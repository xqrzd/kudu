﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Knet.Kudu.Client.Builder;
using Knet.Kudu.Client.Connection;
using Knet.Kudu.Client.Exceptions;
using Knet.Kudu.Client.Internal;
using Knet.Kudu.Client.Protocol;
using Knet.Kudu.Client.Protocol.Consensus;
using Knet.Kudu.Client.Protocol.Master;
using Knet.Kudu.Client.Protocol.Rpc;
using Knet.Kudu.Client.Protocol.Tserver;
using Knet.Kudu.Client.Requests;
using Knet.Kudu.Client.Tablet;
using Knet.Kudu.Client.Util;

namespace Knet.Kudu.Client
{
    public class KuduClient : IAsyncDisposable
    {
        /// <summary>
        /// The number of tablets to fetch from the master in a round trip when
        /// performing a lookup of a single partition (e.g. for a write), or
        /// re-looking-up a tablet with stale information.
        /// </summary>
        private const int FetchTabletsPerPointLookup = 10;
        private const int MaxRpcAttempts = 100;

        public const long NoTimestamp = -1;

        private readonly KuduClientOptions _options;
        private readonly IKuduConnectionFactory _connectionFactory;
        private readonly ConnectionCache _connectionCache;
        private readonly Dictionary<string, TableLocationsCache> _tableLocations;
        private readonly RequestTracker _requestTracker;
        private readonly uint? _defaultOperationTimeoutMs;

        private volatile bool _hasConnectedToMaster;
        private volatile string _location;
        private volatile ServerInfoCache _masterCache;
        private volatile HiveMetastoreConfig _hiveMetastoreConfig;

        /// <summary>
        /// Timestamp required for HybridTime external consistency through timestamp propagation.
        /// </summary>
        private long _lastPropagatedTimestamp = NoTimestamp;
        private readonly object _lastPropagatedTimestampLock = new object();

        public KuduClient(KuduClientOptions options)
        {
            _options = options;
            _connectionFactory = new KuduConnectionFactory(options);
            _connectionCache = new ConnectionCache(_connectionFactory);
            _tableLocations = new Dictionary<string, TableLocationsCache>();
            _requestTracker = new RequestTracker(Guid.NewGuid().ToString("N"));

            if (options.DefaultOperationTimeout.TotalMilliseconds > 0)
                _defaultOperationTimeoutMs = (uint)options.DefaultOperationTimeout.TotalMilliseconds;
        }

        /// <summary>
        /// The last timestamp received from a server. Used for CLIENT_PROPAGATED
        /// external consistency. Note that the returned timestamp is encoded and
        /// cannot be interpreted as a raw timestamp.
        /// </summary>
        public long LastPropagatedTimestamp
        {
            get
            {
                lock (_lastPropagatedTimestampLock)
                {
                    return _lastPropagatedTimestamp;
                }
            }
            set
            {
                lock (_lastPropagatedTimestampLock)
                {
                    if (_lastPropagatedTimestamp == NoTimestamp ||
                        _lastPropagatedTimestamp < value)
                    {
                        _lastPropagatedTimestamp = value;
                    }
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            return _connectionCache.DisposeAsync();
        }

        /// <summary>
        /// Get the Hive Metastore configuration of the most recently connected-to leader master,
        /// or null if the Hive Metastore integration is not enabled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public ValueTask<HiveMetastoreConfig> GetHiveMetastoreConfigAsync(
            CancellationToken cancellationToken = default)
        {
            if (_hasConnectedToMaster)
            {
                return new ValueTask<HiveMetastoreConfig>(_hiveMetastoreConfig);
            }

            return ConnectAsync(cancellationToken);

            async ValueTask<HiveMetastoreConfig> ConnectAsync(CancellationToken cancellationToken)
            {
                await ConnectToClusterAsync(cancellationToken).ConfigureAwait(false);
                return _hiveMetastoreConfig;
            }
        }

        public async Task<KuduTable> CreateTableAsync(TableBuilder table)
        {
            var rpc = new CreateTableRequest(table.Build());
            var response = await SendRpcToMasterAsync(rpc).ConfigureAwait(false);

            await WaitForTableDoneAsync(response.TableId).ConfigureAwait(false);

            var tableIdentifier = new TableIdentifierPB { TableId = response.TableId };
            return await OpenTableAsync(tableIdentifier).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a table on the cluster with the specified name.
        /// </summary>
        /// <param name="tableName">The table's name.</param>
        /// <param name="modifyExternalCatalogs">
        /// Whether to apply the deletion to external catalogs, such as the Hive Metastore.
        /// </param>
        public async Task DeleteTableAsync(string tableName, bool modifyExternalCatalogs = true)
        {
            var request = new DeleteTableRequestPB
            {
                Table = new TableIdentifierPB { TableName = tableName },
                ModifyExternalCatalogs = modifyExternalCatalogs
            };

            var rpc = new DeleteTableRequest(request);

            await SendRpcToMasterAsync(rpc).ConfigureAwait(false);
        }

        public async Task<List<ListTablesResponsePB.TableInfo>> GetTablesAsync(string nameFilter = null)
        {
            var rpc = new ListTablesRequest(nameFilter);
            var response = await SendRpcToMasterAsync(rpc).ConfigureAwait(false);

            return response.Tables;
        }

        public async Task<List<RemoteTablet>> GetTableLocationsAsync(
            string tableId, byte[] partitionKey, uint fetchBatchSize,
            CancellationToken cancellationToken = default)
        {
            // TODO: rate-limit master lookups.

            var request = new GetTableLocationsRequestPB
            {
                Table = new TableIdentifierPB { TableId = tableId.ToUtf8ByteArray() },
                PartitionKeyStart = partitionKey,
                MaxReturnedLocations = fetchBatchSize
            };

            var rpc = new GetTableLocationsRequest(request);
            var result = await SendRpcToMasterAsync(rpc, cancellationToken).ConfigureAwait(false);

            var tabletLocations = new List<RemoteTablet>(result.TabletLocations.Count);

            foreach (var tabletLocation in result.TabletLocations)
            {
                var tablet = await _connectionFactory.GetTabletAsync(tableId, tabletLocation)
                    .ConfigureAwait(false);
                tabletLocations.Add(tablet);
            }

            return tabletLocations;
        }

        public Task<GetTableSchemaResponsePB> GetTableSchemaAsync(string tableName)
        {
            var tableIdentifier = new TableIdentifierPB { TableName = tableName };
            return GetTableSchemaAsync(tableIdentifier);
        }

        public async Task<KuduTable> OpenTableAsync(string tableName)
        {
            var tableIdentifier = new TableIdentifierPB { TableName = tableName };
            var response = await GetTableSchemaAsync(tableIdentifier).ConfigureAwait(false);

            return new KuduTable(response);
        }

        public async Task<WriteResponsePB[]> WriteRowAsync(
            IEnumerable<Operation> operations,
            ExternalConsistencyMode externalConsistencyMode = ExternalConsistencyMode.ClientPropagated)
        {
            var operationsByTablet = new Dictionary<RemoteTablet, List<Operation>>();

            foreach (var operation in operations)
            {
                var tablet = await GetRowTabletAsync(operation).ConfigureAwait(false);

                if (tablet != null)
                {
                    if (!operationsByTablet.TryGetValue(tablet, out var tabletOperations))
                    {
                        tabletOperations = new List<Operation>();
                        operationsByTablet.Add(tablet, tabletOperations);
                    }

                    tabletOperations.Add(operation);
                }
                else
                {
                    // TODO: Handle failure
                    Console.WriteLine("Unable to find tablet");
                }
            }

            var tasks = new Task<WriteResponsePB>[operationsByTablet.Count];
            var i = 0;

            foreach (var tabletOperations in operationsByTablet)
            {
                var task = WriteRowAsync(
                    tabletOperations.Value,
                    tabletOperations.Key,
                    externalConsistencyMode);

                tasks[i++] = task;
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            // TODO: Save timestamp.
            return results;
        }

        private async Task<WriteResponsePB> WriteRowAsync(
            List<Operation> operations,
            RemoteTablet tablet,
            ExternalConsistencyMode externalConsistencyMode)
        {
            var table = operations[0].Table;

            byte[] rowData;
            byte[] indirectData;

            // TODO: Estimate better sizes for these.
            using (var rowBuffer = new BufferWriter(1024))
            using (var indirectBuffer = new BufferWriter(1024))
            {
                OperationsEncoder.Encode(operations, rowBuffer, indirectBuffer);

                // protobuf-net doesn't support serializing Memory<byte>,
                // so we need to copy these into an array.
                rowData = rowBuffer.Memory.ToArray();
                indirectData = indirectBuffer.Memory.ToArray();
            }

            var rowOperations = new RowOperationsPB
            {
                Rows = rowData,
                IndirectData = indirectData
            };

            var request = new WriteRequestPB
            {
                TabletId = tablet.TabletId.ToUtf8ByteArray(),
                Schema = table.SchemaPb.Schema,
                RowOperations = rowOperations,
                ExternalConsistencyMode = (ExternalConsistencyModePB)externalConsistencyMode
            };

            long lastPropagatedTimestamp = LastPropagatedTimestamp;
            if (lastPropagatedTimestamp != NoTimestamp)
            {
                // TODO: This could be different from the one set by SendRpcToTabletAsync()
                request.PropagatedTimestamp = (ulong)lastPropagatedTimestamp;
            }

            var rpc = new WriteRequest(
                request,
                table.TableId,
                tablet.Partition.PartitionKeyStart);

            return await SendRpcToTabletAsync(rpc).ConfigureAwait(false);
        }

        public ScanBuilder NewScanBuilder(KuduTable table)
        {
            return new ScanBuilder(this, table);
        }

        public IKuduSession NewSession(KuduSessionOptions options)
        {
            var session = new KuduSession(this, options);
            session.StartProcessing();
            return session;
        }

        private async Task<KuduTable> OpenTableAsync(TableIdentifierPB tableIdentifier)
        {
            var response = await GetTableSchemaAsync(tableIdentifier).ConfigureAwait(false);

            return new KuduTable(response);
        }

        private async Task<GetTableSchemaResponsePB> GetTableSchemaAsync(TableIdentifierPB tableIdentifier)
        {
            var request = new GetTableSchemaRequestPB { Table = tableIdentifier };
            var rpc = new GetTableSchemaRequest(request);

            return await SendRpcToMasterAsync(rpc).ConfigureAwait(false);
        }

        private async Task WaitForTableDoneAsync(byte[] tableId)
        {
            var request = new IsCreateTableDoneRequestPB
            {
                Table = new TableIdentifierPB { TableId = tableId }
            };

            var rpc = new IsCreateTableDoneRequest(request);

            while (true)
            {
                var result = await SendRpcToMasterAsync(rpc).ConfigureAwait(false);

                if (result.Done)
                    break;

                await Task.Delay(50).ConfigureAwait(false);
                // TODO: Increment rpc attempts.
            }
        }

        internal ValueTask<RemoteTablet> GetRowTabletAsync(Operation operation)
        {
            var table = operation.Table;
            var row = operation.Row;

            using var writer = new BufferWriter(256);
            KeyEncoder.EncodePartitionKey(row, table.PartitionSchema, writer);
            var partitionKey = writer.Memory.Span;

            // Note that we don't have to await this method before disposing the writer, as a
            // copy of partitionKey will be made if the method cannot complete synchronously.
            return GetTabletAsync(table.TableId, partitionKey);
        }

        /// <summary>
        /// Locates a tablet by consulting the table location cache, then by contacting
        /// a master if we haven't seen the tablet before. The results are cached.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested tablet, or null if the tablet doesn't exist.</returns>
        private ValueTask<RemoteTablet> GetTabletAsync(
            string tableId, ReadOnlySpan<byte> partitionKey,
            CancellationToken cancellationToken = default)
        {
            var tablet = GetTabletFromCache(tableId, partitionKey);

            if (tablet != null)
                return new ValueTask<RemoteTablet>(tablet);

            var task = LookupAndCacheTabletAsync(tableId, partitionKey.ToArray(), cancellationToken);
            return new ValueTask<RemoteTablet>(task);
        }

        /// <summary>
        /// Locates a tablet by consulting the table location cache.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>The requested tablet, or null if the tablet doesn't exist.</returns>
        private RemoteTablet GetTabletFromCache(string tableId, ReadOnlySpan<byte> partitionKey)
        {
            TableLocationsCache tableCache;

            lock (_tableLocations)
            {
                if (!_tableLocations.TryGetValue(tableId, out tableCache))
                {
                    // We don't have any tablets cached for this table.
                    return null;
                }
            }

            return tableCache.FindTablet(partitionKey);
        }

        /// <summary>
        /// Locates a tablet by consulting a master and caches the results.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested tablet, or null if the tablet doesn't exist.</returns>
        private async Task<RemoteTablet> LookupAndCacheTabletAsync(
            string tableId, byte[] partitionKey, CancellationToken cancellationToken = default)
        {
            var tablets = await GetTableLocationsAsync(
                tableId,
                partitionKey,
                FetchTabletsPerPointLookup,
                cancellationToken).ConfigureAwait(false);

            CacheTablets(tableId, tablets, partitionKey);

            var tablet = GetTabletFromCache(tableId, partitionKey);

            return tablet;
        }

        /// <summary>
        /// Adds the given tablets to the table location cache.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <param name="tablets">The tablets to cache.</param>
        /// <param name="partitionKey">The partition key used to locate the given tablets.</param>
        private void CacheTablets(string tableId, List<RemoteTablet> tablets, ReadOnlySpan<byte> partitionKey)
        {
            TableLocationsCache cache;

            lock (_tableLocations)
            {
                if (!_tableLocations.TryGetValue(tableId, out cache))
                {
                    cache = new TableLocationsCache();
                    _tableLocations.Add(tableId, cache);
                }
            }

            cache.CacheTabletLocations(tablets, partitionKey);
        }

        private async Task ConnectToClusterAsync(CancellationToken cancellationToken)
        {
            var masterAddresses = _options.MasterAddresses;
            var tasks = new HashSet<Task<ConnectToMasterResponse>>();
            var foundMasters = new List<ServerInfo>(masterAddresses.Count);
            int leaderIndex = -1;

            using var cts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cts.Token, cancellationToken);

            // Attempt to connect to all configured masters in parallel.
            foreach (var address in masterAddresses)
            {
                var task = ConnectToMasterAsync(address, linkedCts.Token);
                tasks.Add(task);
            }

            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(task);

                if (!TryGetConnectResponse(task,
                    out ServerInfo serverInfo,
                    out ConnectToMasterResponsePB responsePb))
                {
                    // Failed to connect to this master.
                    // Failures are fine here, as long as we can
                    // connect to the leader.
                    continue;
                }

                foundMasters.Add(serverInfo);

                if (responsePb.Role == RaftPeerPB.Role.Leader)
                {
                    leaderIndex = foundMasters.Count - 1;

                    _location = responsePb.ClientLocation;

                    var hmsConfig = responsePb.HmsConfig;
                    if (hmsConfig != null)
                    {
                        _hiveMetastoreConfig = new HiveMetastoreConfig(
                            hmsConfig.HmsUris,
                            hmsConfig.HmsSaslEnabled,
                            hmsConfig.HmsUuid);
                    }

                    // Found the leader, that's all we really care about.
                    // Wait a few more seconds to get any followers.
                    cts.CancelAfter(TimeSpan.FromSeconds(3));
                }
            }

            if (leaderIndex != -1)
            {
                _masterCache = new ServerInfoCache(foundMasters, leaderIndex);
                _hasConnectedToMaster = true;
            }
        }

        private async Task<ConnectToMasterResponse> ConnectToMasterAsync(
            HostAndPort hostPort, CancellationToken cancellationToken = default)
        {
            ServerInfo serverInfo = await _connectionFactory.GetServerInfoAsync(
                "master", location: null, hostPort).ConfigureAwait(false);

            var rpc = new ConnectToMasterRequest();
            var response = await SendRpcAsync(rpc, serverInfo, cancellationToken)
                .ConfigureAwait(false);

            return new ConnectToMasterResponse(response, serverInfo);
        }

        private static bool TryGetConnectResponse(
            Task<ConnectToMasterResponse> task,
            out ServerInfo serverInfo,
            out ConnectToMasterResponsePB responsePb)
        {
            serverInfo = null;
            responsePb = null;

            if (!task.IsCompletedSuccessfully())
            {
                // TODO: Log warning.
                Console.WriteLine("Unable to connect to cluster: " +
                    task.Exception.Message);

                return false;
            }

            ConnectToMasterResponse response = task.Result;

            if (response.ResponsePB.Error != null)
            {
                // TODO: Log warning.
                Console.WriteLine("Error connecting to cluster: " +
                    response.ResponsePB.Error.Status.Message);

                return false;
            }

            serverInfo = response.ServerInfo;
            responsePb = response.ResponsePB;

            return true;
        }

        /// <summary>
        /// Sends the provided <see cref="KuduTabletRpc{T}"/> to the server
        /// identified by RPC's table, partition key, and replica selection.
        /// </summary>
        /// <param name="rpc">The RPC to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<T> SendRpcToTabletAsync<T>(
            KuduTabletRpc<T> rpc, CancellationToken cancellationToken = default)
        {
            if (CannotRetryRequest(rpc, cancellationToken))
            {
                // TODO: Change this exception.
                throw new Exception("Attempted Rpc too many times");
            }

            rpc.Attempt++;

            // Set the propagated timestamp so that the next time we send a message to
            // the server the message includes the last propagated timestamp.
            long lastPropagatedTs = LastPropagatedTimestamp;
            if (rpc.ExternalConsistencyMode == ExternalConsistencyMode.ClientPropagated &&
                lastPropagatedTs != NoTimestamp)
            {
                rpc.PropagatedTimestamp = lastPropagatedTs;
            }

            string tableId = rpc.TableId;
            byte[] partitionKey = rpc.PartitionKey;
            RemoteTablet tablet = await GetTabletAsync(tableId, partitionKey)
                .ConfigureAwait(false);
            // TODO: Consider caching non-covered tablet ranges?

            // If we found a tablet, we'll try to find the TS to talk to.
            if (tablet != null)
            {
                ServerInfo serverInfo = GetServerInfo(tablet, rpc.ReplicaSelection);
                if (serverInfo != null)
                {
                    rpc.Tablet = tablet;

                    return await SendRpcAsync(rpc, serverInfo, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            // We fall through to here in two cases:
            //
            // 1) This client has not yet discovered the tablet which is responsible for
            //    the RPC's table and partition key. This can happen when the client's
            //    tablet location cache is cold because the client is new, or the table
            //    is new.
            //
            // 2) The tablet is known, but we do not have an active client for the
            //    leader replica.

            return await SendRpcToTabletAsync(rpc, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> SendRpcToMasterAsync<T>(
            KuduMasterRpc<T> rpc, CancellationToken cancellationToken = default)
        {
            if (CannotRetryRequest(rpc, cancellationToken))
            {
                // TODO: Change this exception.
                throw new Exception("Attempted Rpc too many times");
            }

            rpc.Attempt++;

            // Set the propagated timestamp so that the next time we send a message to
            // the server the message includes the last propagated timestamp.
            long lastPropagatedTs = LastPropagatedTimestamp;
            if (rpc.ExternalConsistencyMode == ExternalConsistencyMode.ClientPropagated &&
                lastPropagatedTs != NoTimestamp)
            {
                rpc.PropagatedTimestamp = lastPropagatedTs;
            }

            ServerInfo serverInfo = GetMasterServerInfo(rpc.ReplicaSelection);
            if (serverInfo != null)
            {
                return await SendRpcAsync(rpc, serverInfo, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await ConnectToClusterAsync(cancellationToken).ConfigureAwait(false);
                return await SendRpcToMasterAsync(rpc, cancellationToken).ConfigureAwait(false);
            }
        }

        private ServerInfo GetServerInfo(RemoteTablet tablet, ReplicaSelection replicaSelection)
        {
            return tablet.GetServerInfo(replicaSelection, _location);
        }

        private ServerInfo GetMasterServerInfo(ReplicaSelection replicaSelection)
        {
            return _masterCache?.GetServerInfo(replicaSelection, _location);
        }

        private Task<T> HandleRetryableErrorAsync<T>(
            KuduMasterRpc<T> rpc, KuduException ex, CancellationToken cancellationToken)
        {
            // TODO: we don't always need to sleep, maybe another replica can serve this RPC.
            return DelayedSendRpcAsync(rpc, ex, cancellationToken);
        }

        private Task<T> HandleRetryableErrorAsync<T>(
             KuduTabletRpc<T> rpc, KuduException ex, CancellationToken cancellationToken)
        {
            // TODO: we don't always need to sleep, maybe another replica can serve this RPC.
            return DelayedSendRpcAsync(rpc, ex, cancellationToken);
        }

        /// <summary>
        /// This methods enable putting RPCs on hold for a period of time determined by
        /// <see cref="DelayRpcAsync{T}(KuduRpc{T}, CancellationToken)"/>. If the RPC is
        /// out of time/retries, an exception is thrown.
        /// </summary>
        /// <param name="rpc">The RPC to retry later.</param>
        /// <param name="exception">The reason why we need to retry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<T> DelayedSendRpcAsync<T>(
            KuduMasterRpc<T> rpc, KuduException exception, CancellationToken cancellationToken)
        {
            if (CannotRetryRequest(rpc, cancellationToken))
            {
                // Don't let it retry.
                ThrowTooManyAttemptsOrTimeoutException(rpc, exception);
            }

            // Here we simply retry the RPC later. We might be doing this along with a lot of other RPCs
            // in parallel. Asynchbase does some hacking with a "probe" RPC while putting the other ones
            // on hold but we won't be doing this for the moment. Regions in HBase can move a lot,
            // we're not expecting this in Kudu.
            await DelayRpcAsync(rpc, cancellationToken).ConfigureAwait(false);
            return await SendRpcToMasterAsync(rpc, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// This methods enable putting RPCs on hold for a period of time determined by
        /// <see cref="DelayRpcAsync{T}(KuduRpc{T}, CancellationToken)"/>. If the RPC is
        /// out of time/retries, an exception is thrown.
        /// </summary>
        /// <param name="rpc">The RPC to retry later.</param>
        /// <param name="exception">The reason why we need to retry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<T> DelayedSendRpcAsync<T>(
            KuduTabletRpc<T> rpc, KuduException exception, CancellationToken cancellationToken)
        {
            if (CannotRetryRequest(rpc, cancellationToken))
            {
                // Don't let it retry.
                ThrowTooManyAttemptsOrTimeoutException(rpc, exception);
            }

            // Here we simply retry the RPC later. We might be doing this along with a lot of other RPCs
            // in parallel. Asynchbase does some hacking with a "probe" RPC while putting the other ones
            // on hold but we won't be doing this for the moment. Regions in HBase can move a lot,
            // we're not expecting this in Kudu.
            await DelayRpcAsync(rpc, cancellationToken).ConfigureAwait(false);
            return await SendRpcToTabletAsync(rpc, cancellationToken).ConfigureAwait(false);
        }

        private async Task<T> SendRpcAsync<T>(
            KuduMasterRpc<T> rpc, ServerInfo serverInfo,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await SendRpcGenericAsync(rpc, serverInfo, cancellationToken)
                    .ConfigureAwait(false);

                if (rpc.Error != null)
                {
                    var code = rpc.Error.Status.Code;
                    var status = KuduStatus.FromMasterErrorPB(rpc.Error);
                    if (rpc.Error.code == MasterErrorPB.Code.NotTheLeader)
                    {
                        //client.handleNotLeader(rpc, new RecoverableException(status), connection.getServerInfo());
                    }
                    else if (code == AppStatusPB.ErrorCode.ServiceUnavailable)
                    {
                        // TODO: This is a crutch until we either don't have to retry RPCs going to the
                        // same server or use retry policies.
                        //client.handleRetryableError(rpc, new RecoverableException(status));
                    }
                    else
                    {
                        throw new NonRecoverableException(status);
                    }
                }

                return result;
            }
            catch (RecoverableException ex)
            {
                Console.WriteLine("Retrying..." + ex);

                return await HandleRetryableErrorAsync(rpc, ex, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task<T> SendRpcAsync<T>(
            KuduTabletRpc<T> rpc, ServerInfo serverInfo,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await SendRpcGenericAsync(rpc, serverInfo, cancellationToken)
                    .ConfigureAwait(false);

                if (rpc.Error != null)
                {
                    var errCode = rpc.Error.code;
                    var errStatusCode = rpc.Error.Status.Code;
                    var status = KuduStatus.FromTabletServerErrorPB(rpc.Error);
                    if (errCode == TabletServerErrorPB.Code.TabletNotFound)
                    {
                        //client.handleTabletNotFound(
                        //    rpc, new RecoverableException(status), connection.getServerInfo());
                        // we're not calling rpc.callback() so we rely on the client to retry that RPC
                    }
                    else if (errCode == TabletServerErrorPB.Code.TabletNotRunning ||
                        errStatusCode == AppStatusPB.ErrorCode.ServiceUnavailable)
                    {
                        //client.handleRetryableError(rpc, new RecoverableException(status));
                        // The following two error codes are an indication that the tablet isn't a leader.
                    }
                    else if (errStatusCode == AppStatusPB.ErrorCode.IllegalState ||
                        errStatusCode == AppStatusPB.ErrorCode.Aborted)
                    {
                        //client.handleNotLeader(rpc, new RecoverableException(status), connection.getServerInfo());
                    }
                    else
                    {
                        throw new NonRecoverableException(status);
                    }
                }

                return result;
            }
            catch (RecoverableException ex)
            {
                Console.WriteLine("Retrying..." + ex);

                return await HandleRetryableErrorAsync(rpc, ex, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task<T> SendRpcGenericAsync<T>(
            KuduRpc<T> rpc, ServerInfo serverInfo,
            CancellationToken cancellationToken = default)
        {
            // This might throw a RecoverableException. That's fine, we'll handle
            // it in whatever method called this one.
            KuduConnection connection = await _connectionCache.GetConnectionAsync(
                serverInfo, cancellationToken).ConfigureAwait(false);

            RequestHeader header = CreateRequestHeader(rpc);

            try
            {
                return await connection.SendReceiveAsync(header, rpc)
                    .ConfigureAwait(false);
            }
            catch (InvalidAuthnTokenException)
            {
                Console.WriteLine("HandleInvalidAuthnToken");
                throw;
            }
            catch (InvalidAuthzTokenException)
            {
                Console.WriteLine("HandleInvalidAuthzToken");
                throw;
            }
            catch (RecoverableException ex)
            {
                if (ex.Status.IsServiceUnavailable)
                {
                    //client.handleRetryableError(rpc, exception);
                }
                else
                {
                    // If we don't really know anything about the exception, invalidate
                    // the location for the tablet, opening the possibility of retrying
                    // on a different server.
                    //client.handleTabletNotFound(rpc, exception, connection.getServerInfo());

                    //invalidateTabletCache(rpc.getTablet(), info, ex.getMessage());
                    //handleRetryableError(rpc, ex);
                }

                throw;
            }
        }

        private RequestHeader CreateRequestHeader<T>(KuduRpc<T> rpc)
        {
            // The callId is set by KuduConnection.SendReceiveAsync().
            var header = new RequestHeader
            {
                // TODO: Add required feature flags
                RemoteMethod = new RemoteMethodPB
                {
                    ServiceName = rpc.ServiceName,
                    MethodName = rpc.MethodName
                }
            };

            // Before we create the request, get an authz token if needed. This is done
            // regardless of whether the KuduRpc object already has a token; we may be
            // a retrying due to an invalid token and the client may have a new token.
            if (rpc.NeedsAuthzToken)
            {
                // TODO: Implement this.
                //rpc.AuthzToken = client.GetAuthzToken(rpc.Table.TableId);
            }

            if (_defaultOperationTimeoutMs.HasValue)
                header.TimeoutMillis = _defaultOperationTimeoutMs.Value;

            if (rpc.IsRequestTracked)
            {
                if (rpc.SequenceId == RequestTracker.NoSeqNo)
                {
                    rpc.SequenceId = _requestTracker.GetNewSeqNo();
                }

                header.RequestId = new RequestIdPB
                {
                    ClientId = _requestTracker.ClientId,
                    SeqNo = rpc.SequenceId,
                    AttemptNo = rpc.Attempt,
                    FirstIncompleteSeqNo = _requestTracker.FirstIncomplete
                };
            }

            return header;
        }

        private async Task DelayRpcAsync<T>(
            KuduRpc<T> rpc, CancellationToken cancellationToken = default)
        {
            int attemptCount = rpc.Attempt;

            if (attemptCount == 0)
            {
                // If this is the first RPC attempt, don't sleep at all.
                return;
            }

            // Randomized exponential backoff, truncated at 4096ms.
            int sleepTime = (int)(Math.Pow(2.0, Math.Min(attemptCount, 12)) *
                ThreadSafeRandom.Instance.NextDouble());

            await Task.Delay(sleepTime, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks whether or not an RPC can be retried once more.
        /// </summary>
        /// <param name="rpc">The RPC we're going to attempt to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private static bool CannotRetryRequest<T>(KuduRpc<T> rpc, CancellationToken cancellationToken)
        {
            return rpc.Attempt > MaxRpcAttempts || cancellationToken.IsCancellationRequested;
        }

        private static void ThrowTooManyAttemptsOrTimeoutException<T>(KuduRpc<T> rpc, KuduException cause)
        {
            string message = rpc.Attempt > MaxRpcAttempts ?
                "Too many attempts." :
                "Request timed out.";

            var statusTimedOut = KuduStatus.TimedOut(message);

            throw new NonRecoverableException(statusTimedOut, cause);
        }

        public static KuduClientBuilder NewBuilder(string masterAddresses)
        {
            return new KuduClientBuilder(masterAddresses);
        }

        public static KuduClientBuilder NewBuilder(IReadOnlyList<HostAndPort> masterAddresses)
        {
            return new KuduClientBuilder(masterAddresses);
        }

        private readonly struct ConnectToMasterResponse
        {
            public ConnectToMasterResponsePB ResponsePB { get; }

            public ServerInfo ServerInfo { get; }

            public ConnectToMasterResponse(
                ConnectToMasterResponsePB responsePB, ServerInfo serverInfo)
            {
                ResponsePB = responsePB;
                ServerInfo = serverInfo;
            }
        }
    }
}
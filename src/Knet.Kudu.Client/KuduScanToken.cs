using System;
using System.Buffers;
using System.Collections.Generic;
using Google.Protobuf;
using Knet.Kudu.Client.Protobuf;
using Knet.Kudu.Client.Protobuf.Client;
using Knet.Kudu.Client.Tablet;

namespace Knet.Kudu.Client
{
    /// <summary>
    /// <para>
    /// A scan token describes a partial scan of a Kudu table limited to a single
    /// contiguous physical location. Using the <see cref="KuduScanTokenBuilder"/>,
    /// clients can describe the desired scan, including predicates, bounds,
    /// timestamps, and caching, and receive back a collection of scan tokens.
    /// </para>
    /// 
    /// <para>
    /// Each scan token may be separately turned into a scanner using
    /// <see cref="IntoScanner{TBuilder}(TBuilder)"/> with each scanner responsible
    /// for a disjoint section of the table.
    /// </para>
    /// 
    /// <para>
    /// Scan tokens may be serialized using the <see cref="Serialize"/> method and
    /// deserialized back into a scanner using the
    /// <see cref="DeserializeIntoScanner{TBuilder}(TBuilder, ReadOnlyMemory{byte})"/>
    /// method. This allows use cases such as generating scan tokens in the planner
    /// component of a query engine, then sending the tokens to execution nodes based
    /// on locality, and then instantiating the scanners on those nodes.
    /// </para>
    /// 
    /// <para>
    /// Scan token locality information can be inspected using <see cref="Tablet"/>.
    /// </para>
    /// </summary>
    public class KuduScanToken
    {
        private readonly KeyRange _keyRange;
        private readonly ScanTokenPB _message;

        public KuduScanToken(KeyRange keyRange, ScanTokenPB message)
        {
            _keyRange = keyRange;
            _message = message;
        }

        public RemoteTablet Tablet => _keyRange.Tablet;

        public byte[] Serialize()
        {
            var writer = new ArrayBufferWriter<byte>();
            _message.WriteTo(writer);
            return writer.WrittenSpan.ToArray();
        }

        public TBuilder IntoScanner<TBuilder>(TBuilder scanBuilder)
            where TBuilder : AbstractKuduScannerBuilder<TBuilder>
        {
            PbIntoScanner(scanBuilder, _message);
            return scanBuilder;
        }

        public override string ToString() => _keyRange.ToString();

        public static TBuilder DeserializeIntoScanner<TBuilder>(
            TBuilder scanBuilder, ReadOnlyMemory<byte> buffer)
            where TBuilder : AbstractKuduScannerBuilder<TBuilder>
        {
            // TODO: Use span when Google.Protobuf 3.18 is released.
            var scanTokenPb = ScanTokenPB.Parser.ParseFrom(buffer.ToArray());
            PbIntoScanner(scanBuilder, scanTokenPb);
            return scanBuilder;
        }

        private static void PbIntoScanner<TBuilder>(
            TBuilder builder, ScanTokenPB scanTokenPb)
            where TBuilder : AbstractKuduScannerBuilder<TBuilder>
        {
            var table = builder.Table;

            if (scanTokenPb.FeatureFlags.Contains(ScanTokenPB.Types.Feature.Unknown))
            {
                throw new Exception("Scan token requires an unsupported feature. " +
                    "This Kudu client must be updated.");
            }

            if (scanTokenPb.HasTableId)
            {
                if (scanTokenPb.TableId != table.TableId)
                {
                    throw new Exception($"Scan token table id {scanTokenPb.TableId} " +
                        $"does not match the builder table id {table.TableId}");
                }
            }
            else
            {
                if (scanTokenPb.TableName != table.TableName)
                {
                    throw new Exception($"Scan token table name {scanTokenPb.TableName} " +
                        $"does not match the builder table name {table.TableName}");
                }
            }

            builder.SetProjectedColumns(
                ComputeProjectedColumnIndexes(scanTokenPb, table.Schema));

            foreach (var predicate in scanTokenPb.ColumnPredicates)
            {
                builder.AddPredicate(KuduPredicate.FromPb(table.Schema, predicate));
            }

            if (scanTokenPb.HasLowerBoundPrimaryKey)
                builder.LowerBoundRaw(scanTokenPb.LowerBoundPrimaryKey.ToByteArray());

            if (scanTokenPb.HasUpperBoundPrimaryKey)
                builder.ExclusiveUpperBoundRaw(scanTokenPb.UpperBoundPrimaryKey.ToByteArray());

            if (scanTokenPb.HasLowerBoundPartitionKey)
                builder.LowerBoundPartitionKeyRaw(scanTokenPb.LowerBoundPartitionKey.ToByteArray());

            if (scanTokenPb.HasUpperBoundPartitionKey)
                builder.ExclusiveUpperBoundPartitionKeyRaw(scanTokenPb.UpperBoundPartitionKey.ToByteArray());

            if (scanTokenPb.HasLimit)
                builder.SetLimit((long)scanTokenPb.Limit);

            if (scanTokenPb.HasReadMode)
            {
                switch (scanTokenPb.ReadMode)
                {
                    case ReadModePB.ReadAtSnapshot:
                        {
                            builder.SetReadMode(ReadMode.ReadAtSnapshot);

                            if (scanTokenPb.HasSnapTimestamp)
                                builder.SnapshotTimestampRaw((long)scanTokenPb.SnapTimestamp);

                            // Set the diff scan timestamps if they are set.
                            if (scanTokenPb.HasSnapStartTimestamp)
                            {
                                builder.DiffScan(
                                    (long)scanTokenPb.SnapStartTimestamp,
                                    (long)scanTokenPb.SnapTimestamp);
                            }

                            break;
                        }
                    case ReadModePB.ReadLatest:
                        {
                            builder.SetReadMode(ReadMode.ReadLatest);
                            break;
                        }
                    case ReadModePB.ReadYourWrites:
                        {
                            builder.SetReadMode(ReadMode.ReadYourWrites);
                            break;
                        }
                    default:
                        throw new Exception("Unknown read mode");
                }
            }

            if (scanTokenPb.HasReplicaSelection)
            {
                switch (scanTokenPb.ReplicaSelection)
                {
                    case ReplicaSelectionPB.LeaderOnly:
                        builder.SetReplicaSelection(ReplicaSelection.LeaderOnly);
                        break;

                    case ReplicaSelectionPB.ClosestReplica:
                        builder.SetReplicaSelection(ReplicaSelection.ClosestReplica);
                        break;

                    default:
                        throw new Exception("Unknown replica selection policy");
                }
            }

            if (scanTokenPb.HasPropagatedTimestamp &&
                (long)scanTokenPb.PropagatedTimestamp != KuduClient.NoTimestamp)
            {
                builder.Client.LastPropagatedTimestamp = (long)scanTokenPb.PropagatedTimestamp;
            }

            if (scanTokenPb.HasCacheBlocks)
                builder.SetCacheBlocks(scanTokenPb.CacheBlocks);

            if (scanTokenPb.HasFaultTolerant)
                builder.SetFaultTolerant(scanTokenPb.FaultTolerant);

            if (scanTokenPb.HasBatchSizeBytes)
                builder.SetBatchSizeBytes((int)scanTokenPb.BatchSizeBytes);

            if (scanTokenPb.HasScanRequestTimeoutMs)
            {
                // TODO
            }

            if (scanTokenPb.HasKeepAlivePeriodMs)
            {
                // TODO
            }
        }

        private static List<int> ComputeProjectedColumnIndexes(
            ScanTokenPB message, KuduSchema schema)
        {
            var columns = new List<int>(message.ProjectedColumns.Count);

            foreach (var colSchemaFromPb in message.ProjectedColumns)
            {
                int colIdx = colSchemaFromPb.HasId && schema.HasColumnIds ?
                    schema.GetColumnIndex((int)colSchemaFromPb.Id) :
                    schema.GetColumnIndex(colSchemaFromPb.Name);

                ColumnSchema colSchema = schema.GetColumn(colIdx);

                if (colSchemaFromPb.Type != (DataTypePB)colSchema.Type)
                {
                    throw new Exception($"Invalid type {colSchemaFromPb.Type} " +
                        $"for column '{colSchemaFromPb.Name}' in scan token, " +
                        $"expected: {colSchema.Type}");
                }

                if (colSchemaFromPb.IsNullable != colSchema.IsNullable)
                {
                    throw new Exception($"Invalid nullability for column '{colSchemaFromPb.Name}' " +
                        $"in scan token, expected: {(colSchema.IsNullable ? "NULLABLE" : "NOT NULL")}");
                }

                columns.Add(colIdx);
            }

            return columns;
        }
    }
}

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Knet.Kudu.Client.Exceptions;
using Knet.Kudu.Client.Internal;
using Knet.Kudu.Client.Logging;
using Knet.Kudu.Client.Protobuf.Rpc;
using Knet.Kudu.Client.Requests;
using Knet.Kudu.Client.Util;
using Microsoft.Extensions.Logging;
using static Knet.Kudu.Client.Protobuf.Rpc.ErrorStatusPB.Types;

namespace Knet.Kudu.Client.Connection
{
    public class KuduConnection
    {
        private readonly IDuplexPipe _ioPipe;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _singleWriter;
        private readonly Dictionary<int, InflightRpc> _inflightRpcs;
        private readonly Task _receiveTask;

        private int _nextCallId;
        private bool _closed;
        private RecoverableException _closedException;
        private DisconnectedCallback _disconnectedCallback;

        public KuduConnection(IDuplexPipe ioPipe, ILoggerFactory loggerFactory)
        {
            _ioPipe = ioPipe;
            _logger = loggerFactory.CreateLogger<KuduConnection>();
            _singleWriter = new SemaphoreSlim(1, 1);
            _inflightRpcs = new Dictionary<int, InflightRpc>();
            _nextCallId = 0;

            _receiveTask = ReceiveAsync();
        }

        public void OnDisconnected(Action<Exception, object> callback, object state)
        {
            lock (_inflightRpcs)
            {
                _disconnectedCallback = new DisconnectedCallback(callback, state);

                if (_closed)
                {
                    // Guarantee the disconnected callback is invoked if
                    // the connection is already closed.
                    InvokeDisconnectedCallback();
                }
            }
        }

        public async Task SendReceiveAsync(
            RequestHeader header,
            KuduRpc rpc,
            CancellationToken cancellationToken)
        {
            var message = new InflightRpc(rpc);
            int callId;

            lock (_inflightRpcs)
            {
                callId = _nextCallId++;
                _inflightRpcs.Add(callId, message);
            }

            header.CallId = callId;

            using var _ = cancellationToken.Register(
                s => ((InflightRpc)s).TrySetCanceled(),
                state: message,
                useSynchronizationContext: false);

            try
            {
                await SendAsync(header, rpc, cancellationToken).ConfigureAwait(false);
                await message.Task.ConfigureAwait(false);
            }
            finally
            {
                RemoveInflightRpc(callId);
            }
        }

        private async ValueTask SendAsync(
            RequestHeader header,
            KuduRpc rpc,
            CancellationToken cancellationToken)
        {
            PipeWriter output = _ioPipe.Output;
            await _singleWriter.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Write(output, header, rpc);

                var result = await output.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                    await output.CompleteAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new RecoverableException(
                    KuduStatus.NetworkError(ex.Message), ex);
            }
            finally
            {
                _singleWriter.Release();
            }
        }

        private static void Write(PipeWriter output, RequestHeader header, KuduRpc rpc)
        {
            // +------------------------------------------------+
            // | Total message length (4 bytes)                 |
            // +------------------------------------------------+
            // | RPC Header protobuf length (variable encoding) |
            // +------------------------------------------------+
            // | RPC Header protobuf                            |
            // +------------------------------------------------+
            // | Main message length (variable encoding)        |
            // +------------------------------------------------+
            // | Main message protobuf                          |
            // +------------------------------------------------+

            var headerSize = header.CalculateSize();
            var messageSize = rpc.CalculateSize();

            var headerSizeLength = CodedOutputStream.ComputeLengthSize(headerSize);
            var messageSizeLength = CodedOutputStream.ComputeLengthSize(messageSize);

            var totalSize = 4 +
                headerSizeLength + headerSize +
                messageSizeLength + messageSize;

            var totalSizeWithoutMessage = totalSize - messageSize;
            var buffer = output.GetSpan(totalSizeWithoutMessage);

            BinaryPrimitives.WriteInt32BigEndian(buffer, totalSize - 4);
            buffer = buffer.Slice(4);

            ProtobufHelper.WriteRawVarint32(buffer, (uint)headerSize);
            buffer = buffer.Slice(headerSizeLength);

            header.WriteTo(buffer.Slice(0, headerSize));
            buffer = buffer.Slice(headerSize);

            ProtobufHelper.WriteRawVarint32(buffer, (uint)messageSize);

            output.Advance(totalSizeWithoutMessage);
            rpc.WriteTo(output);
        }

        private async Task ReceiveAsync()
        {
            var input = _ioPipe.Input;
            var parserContext = new ParserContext();
            Exception exception = null;

            try
            {
                while (true)
                {
                    var result = await input.ReadAsync().ConfigureAwait(false);
                    var buffer = result.Buffer;

                    // TODO: Review this. IsCompleted should happen after ParseMessages().
                    if (result.IsCanceled || result.IsCompleted)
                        break;

                    ParseMessages(buffer, parserContext, out var consumed);

                    input.AdvanceTo(consumed, buffer.End);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                await ShutdownAsync(exception).ConfigureAwait(false);
            }
        }

        private void ParseMessages(
            ReadOnlySequence<byte> buffer,
            ParserContext parserContext,
            out SequencePosition consumed)
        {
            var reader = new SequenceReader<byte>(buffer);

            do
            {
                if (TryParseMessage(ref reader, parserContext))
                {
                    var rpc = parserContext.InflightRpc;
                    var exception = parserContext.Exception;

                    CompleteRpc(rpc, exception);

                    parserContext.Reset();
                }
            } while (parserContext.Step == ParseStep.ReadTotalMessageLength && reader.Remaining >= 4);

            consumed = reader.Position;
        }

        private bool TryParseMessage(
            ref SequenceReader<byte> reader, ParserContext parserContext)
        {
            switch (parserContext.Step)
            {
                case ParseStep.ReadTotalMessageLength:
                    if (parserContext.TryReadTotalMessageLength(ref reader))
                    {
                        goto case ParseStep.ReadHeaderLength;
                    }
                    else
                    {
                        // Not enough data to read the message size.
                        break;
                    }
                case ParseStep.ReadHeaderLength:
                    if (parserContext.TryReadHeaderLength(ref reader))
                    {
                        goto case ParseStep.ReadHeader;
                    }
                    else
                    {
                        // Not enough data to read the header length.
                        parserContext.Step = ParseStep.ReadHeaderLength;
                        break;
                    }
                case ParseStep.ReadHeader:
                    if (parserContext.TryReadResponseHeader(ref reader))
                    {
                        goto case ParseStep.ReadMainMessageLength;
                    }
                    else
                    {
                        // Not enough data to read the header.
                        parserContext.Step = ParseStep.ReadHeader;
                        break;
                    }
                case ParseStep.ReadMainMessageLength:
                    if (parserContext.TryReadMessageLength(ref reader))
                    {
                        goto case ParseStep.ReadProtobufMessage;
                    }
                    else
                    {
                        // Not enough data to read the main message length.
                        parserContext.Step = ParseStep.ReadMainMessageLength;
                        break;
                    }
                case ParseStep.ReadProtobufMessage:
                    {
                        if (!TryGetRpc(parserContext.Header, out parserContext.InflightRpc))
                        {
                            // We don't have this RPC. It probably timed out
                            // and was removed from _inflightRpcs.
                            parserContext.RemainingBytesToSkip = parserContext.MainMessageLength;
                            goto case ParseStep.Skip;
                        }

                        if (!parserContext.TryReadMainMessageProtobuf(
                            ref reader, out var protobufMessage))
                        {
                            // Not enough data to read the main protobuf message.
                            parserContext.Step = ParseStep.ReadProtobufMessage;
                            break;
                        }

                        if (parserContext.Header.IsError)
                        {
                            var error = ProtobufHelper.GetErrorStatus(protobufMessage);
                            var exception = GetException(error);
                            parserContext.Exception = exception;
                        }
                        else
                        {
                            try
                            {
                                parserContext.Rpc.ParseProtobuf(protobufMessage);
                            }
                            catch (Exception ex)
                            {
                                parserContext.Exception = ex;
                            }
                        }

                        if (parserContext.HasSidecars)
                        {
                            var sidecarLength = parserContext.SidecarLength;

                            if (sidecarLength == 0)
                                return true;

                            goto case ParseStep.ReadSidecars;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case ParseStep.ReadSidecars:
                    {
                        if (parserContext.TryReadSidecars(ref reader, out var sidecars))
                        {
                            ProcessSidecars(parserContext, sidecars);
                            return true;
                        }
                        else
                        {
                            // We need more data to finish reading the sidecars.
                            parserContext.Step = ParseStep.ReadSidecars;
                        }

                        break;
                    }
                case ParseStep.Skip:
                    if (!parserContext.TrySkip(ref reader))
                    {
                        // We need more data to skip.
                        parserContext.Step = ParseStep.Skip;
                    }
                    break;
            }

            return false;
        }

        private Exception GetException(ErrorStatusPB error)
        {
            var code = error.Code;
            KuduException exception;

            if (code == RpcErrorCodePB.ErrorServerTooBusy ||
                code == RpcErrorCodePB.ErrorUnavailable)
            {
                exception = new RecoverableException(
                    KuduStatus.ServiceUnavailable(error.Message));
            }
            else if (code == RpcErrorCodePB.FatalInvalidAuthenticationToken)
            {
                exception = new InvalidAuthnTokenException(
                    KuduStatus.NotAuthorized(error.Message));
            }
            else if (code == RpcErrorCodePB.ErrorInvalidAuthorizationToken)
            {
                exception = new InvalidAuthzTokenException(
                    KuduStatus.NotAuthorized(error.Message));
            }
            else
            {
                var message = $"[peer {_ioPipe}] server sent error {error.Message}";
                exception = new RpcRemoteException(KuduStatus.RemoteError(message), error);
            }

            return exception;
        }

        private bool TryGetRpc(ResponseHeader header, out InflightRpc rpc)
        {
            lock (_inflightRpcs)
            {
                return _inflightRpcs.TryGetValue(header.CallId, out rpc);
            }
        }

        private static void CompleteRpc(InflightRpc rpc, Exception exception)
        {
            if (exception is null)
                rpc.TrySetResult();
            else
                rpc.TrySetException(exception);
        }

        private static void ProcessSidecars(ParserContext parserContext, KuduSidecars sidecars)
        {
            try
            {
                parserContext.Rpc.ParseSidecars(sidecars);
            }
            catch (Exception ex)
            {
                parserContext.Exception = ex;
                parserContext.SidecarMemory.Dispose();
            }

            parserContext.SidecarMemory = null;
        }

        private void RemoveInflightRpc(int callId)
        {
            lock (_inflightRpcs)
            {
                _inflightRpcs.Remove(callId);
            }
        }

        /// <summary>
        /// Stops accepting any new RPCs, and completes any outstanding
        /// RPCs with exceptions.
        /// </summary>
        private async Task ShutdownAsync(Exception exception = null)
        {
            await _singleWriter.WaitAsync().ConfigureAwait(false);
            try
            {
                await _ioPipe.Output.CompleteAsync(exception).ConfigureAwait(false);
                await _ioPipe.Input.CompleteAsync(exception).ConfigureAwait(false);
            }
            finally
            {
                _singleWriter.Release();
            }

            if (exception != null)
                _logger.ConnectionDisconnected(_ioPipe.ToString(), exception);

            var closedException = new RecoverableException(KuduStatus.IllegalState(
                $"Connection {_ioPipe} is disconnected."), exception);

            lock (_inflightRpcs)
            {
                _closed = true;
                _closedException = closedException;

                InvokeDisconnectedCallback();

                foreach (var inflightMessage in _inflightRpcs.Values)
                    inflightMessage.TrySetException(closedException);

                _inflightRpcs.Clear();
            }

            // Don't dispose _singleWriter; there may still be
            // pending writes.

            (_ioPipe as IDisposable)?.Dispose();
        }

        public override string ToString() => _ioPipe.ToString();

        /// <summary>
        /// Stops accepting RPCs and completes any outstanding RPCs with exceptions.
        /// </summary>
        public async Task CloseAsync()
        {
            _ioPipe.Output.CancelPendingFlush();
            _ioPipe.Input.CancelPendingRead();

            // Wait for the reader loop to finish.
            await _receiveTask.ConfigureAwait(false);
        }

        /// <summary>
        /// The caller must hold the _inflightMessages lock.
        /// </summary>
        private void InvokeDisconnectedCallback()
        {
            try
            {
                _disconnectedCallback.Invoke(_closedException.InnerException);
            }
            catch { }
        }

        private sealed class InflightRpc : TaskCompletionSource
        {
            public KuduRpc Rpc { get; }

            public InflightRpc(KuduRpc rpc)
                : base(TaskCreationOptions.RunContinuationsAsynchronously)
            {
                Rpc = rpc;
            }
        }

        private readonly struct DisconnectedCallback
        {
            private readonly Action<Exception, object> _callback;
            private readonly object _state;

            public DisconnectedCallback(Action<Exception, object> callback, object state)
            {
                _callback = callback;
                _state = state;
            }

            public void Invoke(Exception exception) => _callback?.Invoke(exception, _state);
        }

        private sealed class ParserContext
        {
            public ParseStep Step;

            /// <summary>
            /// Total message length (4 bytes).
            /// </summary>
            public int TotalMessageLength;

            /// <summary>
            /// RPC Header protobuf length (variable encoding).
            /// </summary>
            public int HeaderLength;

            /// <summary>
            /// Main message length (variable encoding).
            /// Includes the size of any sidecars.
            /// </summary>
            public int MainMessageLength;

            /// <summary>
            /// RPC Header protobuf.
            /// </summary>
            public ResponseHeader Header;

            public InflightRpc InflightRpc;

            public Exception Exception;

            public IMemoryOwner<byte> SidecarMemory;

            public Memory<byte> RemainingSidecarMemory;

            public KuduSidecarOffset[] SidecarOffsets;

            public int RemainingBytesToSkip;

            /// <summary>
            /// Gets the size of the main message protobuf.
            /// </summary>
            public int ProtobufMessageLength => Header.SidecarOffsets.Count == 0 ?
                MainMessageLength : (int)Header.SidecarOffsets[0];

            public bool HasSidecars => Header.SidecarOffsets.Count != 0;

            public int NumSidecars => Header.SidecarOffsets.Count;

            public int SidecarLength => MainMessageLength - (int)Header.SidecarOffsets[0];

            public KuduRpc Rpc => InflightRpc.Rpc;

            public bool TryReadTotalMessageLength(ref SequenceReader<byte> reader)
            {
                return reader.TryReadBigEndian(out TotalMessageLength);
            }

            public bool TryReadHeaderLength(ref SequenceReader<byte> reader)
            {
                return reader.TryReadVarint(out HeaderLength);
            }

            public bool TryReadMessageLength(ref SequenceReader<byte> reader)
            {
                return reader.TryReadVarint(out MainMessageLength);
            }

            public bool TryReadResponseHeader(ref SequenceReader<byte> reader)
            {
                var length = HeaderLength;

                if (reader.Remaining < length)
                {
                    return false;
                }

                var slice = reader.Sequence.Slice(reader.Position, length);
                Header = ResponseHeader.Parser.ParseFrom(slice);

                reader.Advance(length);

                return true;
            }

            public bool TryReadMainMessageProtobuf(
                ref SequenceReader<byte> reader,
                out ReadOnlySequence<byte> message)
            {
                var messageLength = ProtobufMessageLength;

                if (reader.Remaining < messageLength)
                {
                    // Not enough data to parse the main protobuf message.
                    message = default;
                    return false;
                }

                message = reader.Sequence.Slice(reader.Position, messageLength);
                reader.Advance(messageLength);

                return true;
            }

            public bool TrySkip(ref SequenceReader<byte> reader)
            {
                var remainingBytesToSkip = RemainingBytesToSkip;
                var bytesToSkip = Math.Min((int)reader.Remaining, remainingBytesToSkip);

                reader.Advance(bytesToSkip);
                remainingBytesToSkip -= bytesToSkip;
                RemainingBytesToSkip = remainingBytesToSkip;

                return remainingBytesToSkip == 0;
            }

            public bool TryReadSidecars(
                ref SequenceReader<byte> reader,
                out KuduSidecars sidecars)
            {
                var sidecar = GetSidecar();
                var desiredLength = sidecar.Length;
                var availableLength = (int)reader.Remaining;

                var readLength = Math.Min(desiredLength, availableLength);
                var slice = sidecar.Slice(0, readLength);

                reader.TryCopyTo(slice);
                reader.Advance(readLength);

                if (desiredLength <= availableLength)
                {
                    // We've read all the sidecar data for this RPC.
                    sidecars = new KuduSidecars(
                        SidecarMemory,
                        SidecarOffsets,
                        SidecarLength);

                    return true;
                }

                AdvanceSidecar(readLength);

                sidecars = default;
                return false;
            }

            private Span<byte> GetSidecar()
            {
                var sidecarMemory = SidecarMemory;

                if (sidecarMemory is null)
                {
                    // Setup for processing sidecars.
                    // TODO: Allow MemoryPool to be passed in.
                    var sidecarLength = SidecarLength;
                    sidecarMemory = MemoryPool<byte>.Shared.Rent(sidecarLength);

                    SidecarMemory = sidecarMemory;
                    RemainingSidecarMemory = sidecarMemory.Memory.Slice(0, sidecarLength);

                    SetSidecarOffsets();
                }

                return RemainingSidecarMemory.Span;
            }

            private void SetSidecarOffsets()
            {
                var rawSidecarOffsets = Header.SidecarOffsets;
                int numSidecars = rawSidecarOffsets.Count;
                int totalSize = 0;

                var sidecarOffsets = new KuduSidecarOffset[numSidecars];

                for (int i = 0; i < numSidecars - 1; i++)
                {
                    var currentOffset = rawSidecarOffsets[i];
                    var nextOffset = rawSidecarOffsets[i + 1];
                    int size = (int)(nextOffset - currentOffset);

                    var offset = new KuduSidecarOffset(totalSize, size);
                    sidecarOffsets[i] = offset;

                    totalSize += size;
                }

                // Handle the last sidecar.
                var remainingSize = SidecarLength - totalSize;
                var lastOffset = new KuduSidecarOffset(totalSize, remainingSize);
                sidecarOffsets[numSidecars - 1] = lastOffset;

                SidecarOffsets = sidecarOffsets;
            }

            private void AdvanceSidecar(int read)
            {
                RemainingSidecarMemory = RemainingSidecarMemory.Slice(read);
            }

            public void Reset()
            {
                Step = ParseStep.ReadTotalMessageLength;
                TotalMessageLength = default;
                HeaderLength = default;
                MainMessageLength = default;
                Header = default;
                InflightRpc = default;
                Exception = default;
                SidecarMemory?.Dispose();
                SidecarMemory = default;
                RemainingSidecarMemory = default;
                SidecarOffsets = default;
                RemainingBytesToSkip = default;
            }
        }

        private enum ParseStep
        {
            /// <summary>
            /// Total message length (4 bytes).
            /// </summary>
            ReadTotalMessageLength,
            /// <summary>
            /// RPC Header protobuf length (variable encoding).
            /// </summary>
            ReadHeaderLength,
            /// <summary>
            /// RPC Header protobuf.
            /// </summary>
            ReadHeader,
            /// <summary>
            /// Main message length (variable encoding).
            /// </summary>
            ReadMainMessageLength,
            /// <summary>
            /// Main message protobuf.
            /// </summary>
            ReadProtobufMessage,
            /// <summary>
            /// Variable number of sidecars, defined in the RPC header.
            /// </summary>
            ReadSidecars,
            /// <summary>
            /// We couldn't match the incoming CallId to an inflight RPC,
            /// probably because the RPC timed out (on our side) and was
            /// removed. Skip the remaining data for the message.
            /// </summary>
            Skip
        }
    }
}

﻿using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial;

namespace Kudu.Client.Connection
{
    public class KuduSocketConnection : IDuplexPipe, IDisposable
    {
        private readonly Socket _socket;
        private readonly IDuplexPipe _ioPipe;

        public KuduSocketConnection(Socket socket, IDuplexPipe ioPipe)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _ioPipe = ioPipe ?? throw new ArgumentNullException(nameof(ioPipe));
        }

        public PipeReader Input => _ioPipe.Input;

        public PipeWriter Output => _ioPipe.Output;

        public override string ToString() => _ioPipe.ToString();

        public void Dispose()
        {
            try { _ioPipe.Input.CancelPendingRead(); } catch { }
            try { _ioPipe.Input.Complete(); } catch { }
            try { _ioPipe.Output.CancelPendingFlush(); } catch { }
            try { _ioPipe.Output.Complete(); } catch { }

            try { using (_ioPipe as IDisposable) { } } catch { }

            try { _socket.Shutdown(SocketShutdown.Both); } catch { }
            try { _socket.Close(); } catch { }
            try { _socket.Dispose(); } catch { }
        }

        public static async Task<Socket> ConnectAsync(IPEndPoint endpoint)
        {
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketConnection.SetRecommendedClientOptions(socket);

            await socket.ConnectAsync(endpoint).ConfigureAwait(false);
            return socket;
        }
    }
}

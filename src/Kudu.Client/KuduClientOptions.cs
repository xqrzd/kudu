﻿using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using Kudu.Client.Connection;
using Pipelines.Sockets.Unofficial;

namespace Kudu.Client
{
    public class KuduClientOptions
    {
        public IReadOnlyList<HostAndPort> MasterAddresses { get; }

        public string KerberosSpn { get; }

        public string TlsHost { get; }

        public TimeSpan DefaultAdminOperationTimeout { get; }

        public TimeSpan DefaultOperationTimeout { get; }

        public PipeOptions SendPipeOptions { get; }

        public PipeOptions ReceivePipeOptions { get; }

        public KuduClientOptions(
            IReadOnlyList<HostAndPort> masterAddresses,
            string kerberosSpn,
            string tlsHost,
            TimeSpan defaultAdminOperationTimeout,
            TimeSpan defaultOperationTimeout,
            PipeOptions sendPipeOptions,
            PipeOptions receivePipeOptions)
        {
            MasterAddresses = masterAddresses;
            KerberosSpn = kerberosSpn;
            TlsHost = tlsHost;
            DefaultAdminOperationTimeout = defaultAdminOperationTimeout;
            DefaultOperationTimeout = defaultOperationTimeout;

            SendPipeOptions = sendPipeOptions ?? StaticContext.DefaultSendOptions;
            ReceivePipeOptions = receivePipeOptions ?? StaticContext.DefaultReceiveOptions;
        }

        private static class StaticContext
        {
            // Place these here so we don't instantiate them too early.

            internal static readonly PipeOptions DefaultSendOptions = new PipeOptions(
                readerScheduler: DedicatedThreadPoolPipeScheduler.Default,
                writerScheduler: DedicatedThreadPoolPipeScheduler.Default,
                pauseWriterThreshold: 1024 * 1024 * 4,  // 4MB
                resumeWriterThreshold: 1024 * 1024 * 2, // 2MB
                minimumSegmentSize: 4096,
                useSynchronizationContext: false);

            internal static readonly PipeOptions DefaultReceiveOptions = new PipeOptions(
                readerScheduler: DedicatedThreadPoolPipeScheduler.Default,
                writerScheduler: DedicatedThreadPoolPipeScheduler.Default,
                pauseWriterThreshold: 1024 * 1024 * 256,  // 256MB
                resumeWriterThreshold: 1024 * 1024 * 128, // 128MB
                minimumSegmentSize: 1024 * 256,
                useSynchronizationContext: false);
        }
    }
}

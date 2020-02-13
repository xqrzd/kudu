﻿using Knet.Kudu.Client.Scanner;
using Microsoft.Extensions.Logging;

namespace Knet.Kudu.Client
{
    public class KuduScannerBuilder :
        AbstractKuduScannerBuilder<KuduScannerBuilder, KuduScanner<ResultSet>>
    {
        public KuduScannerBuilder(KuduClient client, KuduTable table, ILogger logger)
            : base(client, table, logger)
        {
        }

        public override KuduScanner<ResultSet> Build()
        {
            return new KuduScanner<ResultSet>(
                Logger,
                Client,
                Table,
                new ResultSetScanParser(),
                ProjectedColumns,
                Predicates,
                ReadMode,
                ReplicaSelection,
                IsFaultTolerant,
                BatchSizeBytes,
                Limit,
                CacheBlocks,
                StartTimestamp,
                HtTimestamp,
                LowerBoundPrimaryKey,
                UpperBoundPrimaryKey,
                LowerBoundPartitionKey,
                UpperBoundPartitionKey);
        }
    }
}

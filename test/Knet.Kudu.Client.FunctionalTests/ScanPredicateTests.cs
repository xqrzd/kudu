﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Knet.Kudu.Client.FunctionalTests.MiniCluster;
using Knet.Kudu.Client.FunctionalTests.Util;
using McMaster.Extensions.Xunit;
using Xunit;

namespace Knet.Kudu.Client.FunctionalTests
{
    [MiniKuduClusterTest]
    public class ScanPredicateTests : IAsyncLifetime
    {
        private readonly KuduTestHarness _harness;
        private readonly KuduClient _client;
        private readonly IKuduSession _session;

        public ScanPredicateTests()
        {
            _harness = new MiniKuduClusterBuilder().BuildHarness();
            _client = _harness.CreateClient();
            _session = _client.NewSession();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _session.DisposeAsync();
            await _client.DisposeAsync();
            await _harness.DisposeAsync();
        }

        [SkippableFact]
        public async Task TestBoolPredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("bool-table")
                .AddColumn("value", KuduType.Bool);

            var table = await _client.CreateTableAsync(builder);

            var values = new SortedSet<bool> { true, false };

            long i = 0;
            foreach (var value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetBool("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            var col = table.Schema.GetColumn("value");
            Assert.Equal(values.Count + 1, await CountRowsAsync(table));

            foreach (var v in values)
            {
                // value = v
                var equal = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.Equal, v);
                Assert.Equal(values.Contains(v) ? 1 : 0, await CountRowsAsync(table, equal));

                // value >= v
                var greaterEqual = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.GreaterEqual, v);
                Assert.Equal(values.TailSet(v).Count, await CountRowsAsync(table, greaterEqual));

                // value <= v
                var lessEqual = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.LessEqual, v);
                Assert.Equal(values.HeadSet(v, true).Count, await CountRowsAsync(table, lessEqual));

                // value > v
                var greater = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.Greater, v);
                Assert.Equal(values.TailSet(v, false).Count, await CountRowsAsync(table, greater));

                // value < v
                var less = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.Less, v);
                Assert.Equal(values.HeadSet(v).Count, await CountRowsAsync(table, less));
            }
        }

        private TableBuilder GetDefaultTableBuilder()
        {
            return new TableBuilder()
                .AddColumn("key", KuduType.Int64, opt => opt.Key(true))
                .SetRangePartitionColumns("key");
        }

        private async Task<int> CountRowsAsync(KuduTable table, params KuduPredicate[] predicates)
        {
            var scanBuilder = _client.NewScanBuilder(table);

            foreach (var predicate in predicates)
                scanBuilder.AddPredicate(predicate);

            var scanner = scanBuilder.Build();
            int count = 0;

            await foreach (var resultSet in scanner)
            {
                count += resultSet.Count;
            }

            return count;
        }
    }
}
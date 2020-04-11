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

        [SkippableFact]
        public async Task TestBytePredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("byte-table")
                .AddColumn("value", KuduType.Int8);

            var table = await _client.CreateTableAsync(builder);

            var values = CreateIntegerValues(KuduType.Int8);

            long i = 0;
            foreach (byte value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetByte("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            await CheckIntPredicatesAsync(table, values, CreateIntegerTestValues(KuduType.Int8));
        }

        [SkippableFact]
        public async Task TestShortPredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("short-table")
                .AddColumn("value", KuduType.Int16);

            var table = await _client.CreateTableAsync(builder);

            var values = CreateIntegerValues(KuduType.Int16);

            long i = 0;
            foreach (short value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetInt16("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            await CheckIntPredicatesAsync(table, values, CreateIntegerTestValues(KuduType.Int16));
        }

        [SkippableFact]
        public async Task TestIntPredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("int-table")
                .AddColumn("value", KuduType.Int32);

            var table = await _client.CreateTableAsync(builder);

            var values = CreateIntegerValues(KuduType.Int32);

            long i = 0;
            foreach (int value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetInt32("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            await CheckIntPredicatesAsync(table, values, CreateIntegerTestValues(KuduType.Int32));
        }

        [SkippableFact]
        public async Task TestLongPredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("long-table")
                .AddColumn("value", KuduType.Int64);

            var table = await _client.CreateTableAsync(builder);

            var values = CreateIntegerValues(KuduType.Int64);

            long i = 0;
            foreach (long value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetInt64("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            await CheckIntPredicatesAsync(table, values, CreateIntegerTestValues(KuduType.Int64));
        }

        [SkippableFact]
        public async Task TestFloatPredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("float-table")
                .AddColumn("value", KuduType.Float);

            var table = await _client.CreateTableAsync(builder);

            var values = CreateFloatValues();
            var testValues = CreateFloatTestValues();

            long i = 0;
            foreach (var value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetFloat("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            var col = table.Schema.GetColumn("value");
            Assert.Equal(values.Count + 1, await CountRowsAsync(table));

            foreach (var v in testValues)
            {
                // value = v
                var equal = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.Equal, v);
                Assert.Equal(values.GetViewBetween(v, v).Count, await CountRowsAsync(table, equal));

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

            var isNotNull = KuduPredicate.NewIsNotNullPredicate(col);
            Assert.Equal(values.Count, await CountRowsAsync(table, isNotNull));

            var isNull = KuduPredicate.NewIsNullPredicate(col);
            Assert.Equal(1, await CountRowsAsync(table, isNull));
        }

        [SkippableFact]
        public async Task TestDoublePredicates()
        {
            var builder = GetDefaultTableBuilder()
                .SetTableName("double-table")
                .AddColumn("value", KuduType.Double);

            var table = await _client.CreateTableAsync(builder);

            var values = CreateDoubleValues();
            var testValues = CreateDoubleTestValues();

            long i = 0;
            foreach (var value in values)
            {
                var insert = table.NewInsert();
                insert.SetInt64("key", i++);
                insert.SetDouble("value", value);
                await _session.EnqueueAsync(insert);
            }

            var nullInsert = table.NewInsert();
            nullInsert.SetInt64("key", i);
            nullInsert.SetNull("value");
            await _session.EnqueueAsync(nullInsert);
            await _session.FlushAsync();

            var col = table.Schema.GetColumn("value");
            Assert.Equal(values.Count + 1, await CountRowsAsync(table));

            foreach (var v in testValues)
            {
                // value = v
                var equal = KuduPredicate.NewComparisonPredicate(col, ComparisonOp.Equal, v);
                Assert.Equal(values.GetViewBetween(v, v).Count, await CountRowsAsync(table, equal));

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

            var isNotNull = KuduPredicate.NewIsNotNullPredicate(col);
            Assert.Equal(values.Count, await CountRowsAsync(table, isNotNull));

            var isNull = KuduPredicate.NewIsNullPredicate(col);
            Assert.Equal(1, await CountRowsAsync(table, isNull));
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

        private SortedSet<long> CreateIntegerValues(KuduType type)
        {
            var values = new SortedSet<long>();
            for (long i = -50; i < 50; i++)
            {
                values.Add(i);
            }
            values.Add(KuduPredicate.MinIntValue(type));
            values.Add(KuduPredicate.MinIntValue(type) + 1);
            values.Add(KuduPredicate.MaxIntValue(type) - 1);
            values.Add(KuduPredicate.MaxIntValue(type));
            return values;
        }

        private List<long> CreateIntegerTestValues(KuduType type)
        {
            return new List<long>
            {
                KuduPredicate.MinIntValue(type),
                KuduPredicate.MinIntValue(type) + 1,
                -51L,
                50L,
                0L,
                49L,
                50L,
                KuduPredicate.MaxIntValue(type) - 1,
                KuduPredicate.MaxIntValue(type)
            };
        }

        private SortedSet<float> CreateFloatValues()
        {
            var values = new SortedSet<float>();
            for (long i = -50; i < 50; i++)
            {
                values.Add(i + i / 100.0f);
            }

            values.Add(float.NegativeInfinity);
            values.Add(-float.MaxValue);
            values.Add(-float.Epsilon);
            values.Add(-float.MinValue);
            values.Add(float.MinValue);
            values.Add(float.Epsilon);
            values.Add(float.MaxValue);
            values.Add(float.PositiveInfinity);

            return values;
        }

        private List<float> CreateFloatTestValues()
        {
            return new List<float>
            {
                float.NegativeInfinity,
                -float.MaxValue,
                -100.0F,
                -1.1F,
                -1.0F,
                -float.Epsilon,
                -float.MinValue,
                0.0F,
                float.MinValue,
                float.Epsilon,
                1.0F,
                1.1F,
                100.0F,
                float.MaxValue,
                float.PositiveInfinity
            };
        }

        private SortedSet<double> CreateDoubleValues()
        {
            var values = new SortedSet<double>();
            for (long i = -50; i < 50; i++)
            {
                values.Add(i + i / 100.0);
            }

            values.Add(double.NegativeInfinity);
            values.Add(-double.MaxValue);
            values.Add(-double.Epsilon);
            values.Add(-double.MinValue);
            values.Add(double.MinValue);
            values.Add(double.Epsilon);
            values.Add(double.MaxValue);
            values.Add(double.PositiveInfinity);

            return values;
        }

        private List<double> CreateDoubleTestValues()
        {
            return new List<double>
            {
                double.NegativeInfinity,
                -double.MaxValue,
                -100.0,
                -1.1,
                -1.0,
                -double.Epsilon,
                -double.MinValue,
                0.0,
                double.MinValue,
                double.Epsilon,
                1.0,
                1.1,
                100.0,
                double.MaxValue,
                double.PositiveInfinity
            };
        }

        private async Task CheckIntPredicatesAsync(
            KuduTable table,
            SortedSet<long> values,
            List<long> testValues)
        {
            var col = table.Schema.GetColumn("value");
            Assert.Equal(values.Count + 1, await CountRowsAsync(table));

            foreach (var v in testValues)
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

            var isNotNull = KuduPredicate.NewIsNotNullPredicate(col);
            Assert.Equal(values.Count, await CountRowsAsync(table, isNotNull));

            var isNull = KuduPredicate.NewIsNullPredicate(col);
            Assert.Equal(1, await CountRowsAsync(table, isNull));
        }
    }
}

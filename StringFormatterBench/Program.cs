﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Formatting;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace StringFormatterBench
{
    public class Config : ManualConfig
    {
        public Config() {
            Add(Job.LegacyJitX64, Job.RyuJitX64);
            Add(StatisticColumn.Min);
            Add(StatisticColumn.Max);
            Add(new MemoryDiagnoser());
        }
    }

    [Config(typeof(Config))]
    public unsafe class StringFormatBenchmark {
        StringBuilder _sb1;
        StringBuffer _sb2;
        char* _dest;
        static readonly string formatTest = "Foo {0,13:e12} and bar!! {1,-15:P}bah";
        const double v1 = 13.934939;
        const double v2 = 0;

        [Setup]
        public void Setup() {
            _dest = (char*) Marshal.AllocHGlobal(128);
            _sb1 = new StringBuilder();
            _sb2 = new StringBuffer();
        }

        [Benchmark(Baseline = true)]
        public string Baseline() {
            _sb1.AppendFormat(formatTest, v1, v2);
            var s = _sb1.ToString();
            _sb1.Clear();
            return s;
        }

        [Benchmark]
        public string StringBuffer() {
            _sb2.AppendFormat(formatTest, v1, v2);
            var s = _sb2.ToString();
            _sb2.Clear();
            return s;
        }
    }

    [Config(typeof(Config))]
    public unsafe class NoAllocationBenchmark {
        StringBuilder _sb1;
        StringBuffer _sb2;
        public char* DestNative;
        public char[] DestManaged;
        static readonly string formatTest = "Foo {0,13:e12} and bar!! {1,-15:P}bah";

        const double v1 = 13.934939;
        const double v2 = 0;

        [Setup]
        public void Setup() {
            DestNative = (char*)Marshal.AllocHGlobal(128);
            DestManaged = new char[128];
            _sb1 = new StringBuilder();
            _sb2 = new StringBuffer();
        }

        [Benchmark(Baseline = true)]
        public void Baseline() {
            _sb1.AppendFormat(formatTest, v1, v2);
            _sb1.CopyTo(0, DestManaged, 0, _sb1.Length);
            _sb1.Clear();
        }

        [Benchmark]
        public void NoAllocation() {
            _sb2.AppendFormat(formatTest, v1, v2);
            _sb2.CopyTo(DestNative, 0, _sb2.Count);
            _sb2.Clear();
        }
    }

    public class DateDeconstructBenchmark
    {
        private DateTime _test;
        public DateDeconstructBenchmark()
        {
            _test = DateTime.UtcNow;
        }

        [Benchmark(Baseline = true)]
        public int BaseLine() => _test.Year + _test.Month + _test.Day;

        [Benchmark]
        public int Test()
        {
            var (y, m, d) = _test;
            return y + m + d;
        }

        [Benchmark]
        public int BaseLongLine() => _test.Year + _test.Month + _test.Day + _test.Hour + _test.Minute + _test.Second + _test.Millisecond;

        [Benchmark]
        public int TestLong()
        {
            var (y, m, d, h, mi, s, ms) = _test;
            return y + m + d + h + mi + s + ms;
        }
    }

    class Program {
        static void Main(string[] args) {
            var competition = new BenchmarkSwitcher(new[] {
                typeof(StringFormatBenchmark),
                typeof(NoAllocationBenchmark),
                typeof(DateDeconstructBenchmark),
            });

            competition.Run(args);
        }
    }
}

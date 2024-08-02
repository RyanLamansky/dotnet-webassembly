using Benchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

BenchmarkSwitcher benchmark = BenchmarkSwitcher.FromTypes([
    typeof(CompareWasmtimeDotnet),
]);

IConfig configuration = DefaultConfig.Instance;

if (args.Length > 0)
{
    benchmark.Run(args, configuration);
}
else
{
    benchmark.RunAll(configuration);
}
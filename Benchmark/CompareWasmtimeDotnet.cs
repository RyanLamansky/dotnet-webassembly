using BenchmarkDotNet.Attributes;
using Wasmtime;
using WebAssembly;
using WebAssembly.Runtime;

namespace Benchmark;

public class CompareWasmtimeDotnet
{
    private Instance<Exports> _dotnet = null!;
    private Instance<dynamic> _dotnetDynamic = null!;
    private Func<int> _wasmtime = null!;

    [GlobalSetup]
    public void Setup()
    {
        const string path = "wasm/inthrustwetrust.wasm";

        LoadDotnetWebassembly(path);
        LoadWasmtimeDotnet(path);
    }

    private void LoadDotnetWebassembly(string path)
    {
        using var stream = File.OpenRead(path);
        _dotnet = Compile.FromBinary<Exports>(stream)(new ImportDictionary());

        stream.Position = 0;
        _dotnetDynamic = Compile.FromBinary<dynamic>(stream)(new ImportDictionary());
    }

    private void LoadWasmtimeDotnet(string path)
    {
        Engine engine = new(new Config());

        var module = Wasmtime.Module.FromFile(engine, path);
        var store = new Store(engine);

        var instance = new Instance(store, module);
        _wasmtime = instance.GetFunction<int>("bench") ?? throw new InvalidOperationException("Missing 'bench' function");
    }

    [Benchmark]
    public void DotnetWebassembly()
    {
        _dotnet.Exports.bench();
    }

    [Benchmark]
    public void DotnetWebassemblyDynamic()
    {
        _dotnetDynamic.Exports.bench();
    }

    [Benchmark]
    public void WasmtimeDotnet()
    {
        _wasmtime();
    }

    public abstract class Exports
    {
        public abstract int bench();
    }
}
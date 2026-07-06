namespace WebAssembly.Instructions;

/// <summary>Shared module scaffolds for the WASM 3.0 GC per-instruction execution tests.</summary>
internal static class GCTestHelper
{
    /// <summary>The export name every GC test module uses.</summary>
    public const string ExportName = "Test";

    /// <summary>
    /// Builds a single-exported-function module whose type 0 is a mutable-<c>i32</c> array and type 1 is the
    /// given function signature; the export references type 1 and runs the supplied body.
    /// </summary>
    public static Module MutableInt32ArrayModule(WebAssemblyType signature, FunctionBody body)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Array, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] });
        module.Types.Add(signature);
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(ExportName, 0));
        module.Codes.Add(body);
        return module;
    }

    /// <summary>Builds a single-exported-function module whose only type (index 0) is the given signature.</summary>
    public static Module FunctionModule(WebAssemblyType signature, FunctionBody body)
    {
        var module = new Module();
        module.Types.Add(signature);
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(ExportName, 0));
        module.Codes.Add(body);
        return module;
    }
}

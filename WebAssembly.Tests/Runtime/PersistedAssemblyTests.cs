#if NET9_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the WASM-to-DLL path (<see cref="Compile.CreatePersistedAssembly"/>), which emits a standalone
/// assembly instead of an in-memory instance.
/// </summary>
[TestClass]
public class PersistedAssemblyTests
{
    static (Type type, object instance) CompileLoadAndInstantiate(Module module, out PersistedCompilerConfiguration configuration)
    {
        using var wasm = new MemoryStream();
        module.WriteToBinary(wasm);
        wasm.Position = 0;

        configuration = new PersistedCompilerConfiguration(
            typeof(object).Assembly,
            typeof(Compile).Assembly,
            new AssemblyName("WasmToDllTest"),
            "WasmToDllTestModule");

        var builder = Compile.CreatePersistedAssembly(wasm, configuration);

        using var dll = new MemoryStream();
        builder.Save(dll);

        var assembly = Assembly.Load(dll.ToArray());
        var type = assembly.GetType(configuration.TypeName);
        Assert.IsNotNull(type, $"Expected type {configuration.TypeName} in the generated assembly.");

        // The generated type's constructor takes the import resolver; this module has no imports.
        var instance = Activator.CreateInstance(type,
            (Func<string, string, RuntimeImport>)((module, field) => throw new InvalidOperationException($"Unexpected import {module}::{field}.")));
        Assert.IsNotNull(instance);
        return (type, instance);
    }

    /// <summary>
    /// Full round-trip: compile a function to a DLL, load it, and invoke the exported method.
    /// </summary>
    [TestMethod]
    public void PersistedAssembly_Function_RoundTrip()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function { Type = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code = [new LocalGet(0), new LocalGet(1), new Int32Add(), new End()],
        });
        module.Exports.Add(new Export { Name = "Add", Index = 0 });

        var (type, instance) = CompileLoadAndInstantiate(module, out _);

        var add = type.GetMethod("Add");
        Assert.IsNotNull(add);
        Assert.AreEqual(5, add.Invoke(instance, [2, 3]));
    }

    /// <summary>
    /// An exported memory survives the WASM-to-DLL path as an accessible property. This exercises the
    /// table/memory export emission, whose export-attribute placement differs from the in-memory path.
    /// </summary>
    [TestMethod]
    public void PersistedAssembly_MemoryExport_RoundTrip()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Exports.Add(new Export { Kind = ExternalKind.Memory, Name = "mem", Index = 0 });

        var (type, instance) = CompileLoadAndInstantiate(module, out _);

        var memory = type.GetProperty("mem");
        Assert.IsNotNull(memory, "Expected an exported 'mem' property on the generated type.");
        Assert.IsNotNull(memory.GetValue(instance), "The exported memory should be instantiated.");
    }
}
#endif

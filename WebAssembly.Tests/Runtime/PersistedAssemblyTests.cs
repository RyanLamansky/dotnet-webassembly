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

    /// <summary>
    /// A module that defines a tag, throws it, and catches it in a try_table survives the WASM-to-DLL path,
    /// exercising the <see cref="ExceptionTag"/>/<see cref="WebAssemblyException"/> type references and the
    /// filter/handler IL under the persisted <c>NeutralizeType</c> mapping.
    /// </summary>
    [TestMethod]
    public void PersistedAssembly_TagThrowCatch_RoundTrip()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        }); // 1: (i32) -> (i32)
        module.Tags.Add(new Tag(0));
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = "f", Index = 0 });

        var tryTable = new TryTable(BlockType.Int32);
        tryTable.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Empty),
                    tryTable,
                        new LocalGet(0),
                        new Int32EqualZero(),
                        new If(BlockType.Empty),
                            new Throw(0),
                        new End(),
                        new Int32Constant(42),
                    new End(),
                    new Return(),
                new End(),
                new Int32Constant(23),
                new End(),
            ],
        });

        var (type, instance) = CompileLoadAndInstantiate(module, out _);

        var f = type.GetMethod("f");
        Assert.IsNotNull(f);
        Assert.AreEqual(42, f.Invoke(instance, [1]), "No throw: the try body's result flows out.");
        Assert.AreEqual(23, f.Invoke(instance, [0]), "Throw caught: control reaches the catch label.");
    }

    /// <summary>
    /// A module that allocates a garbage-collected struct, downcasts it through the abstract and concrete
    /// <c>ref.cast</c> forms, and reads a field survives the WASM-to-DLL path. This exercises the emitted GC class,
    /// its parameterless constructor, and the <see cref="WebAssemblyStructObject"/>/<see cref="WebAssemblyI31"/> references under
    /// the persisted <c>NeutralizeType</c> mapping (the M8.6 cast lowering).
    /// </summary>
    [TestMethod]
    public void PersistedAssembly_StructCast_RoundTrip()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Form = FunctionType.Struct,
            Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }],
        }); // 0: (struct (field i32))
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] }); // 1: () -> (i32)
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = "f", Index = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(42),
                new StructNew { Type = 0 },                        // (ref 0)
                new RefCast { HeapType = WebAssemblyHeapType.Struct }, // (ref struct) via WebAssemblyStructObject
                new RefCast { TypeIndex = 0 },                     // back to (ref 0) via the emitted class
                new StructGet { Type = 0, Field = 0 },             // 42
                new End(),
            ],
        });

        var (type, instance) = CompileLoadAndInstantiate(module, out _);

        var f = type.GetMethod("f");
        Assert.IsNotNull(f);
        Assert.AreEqual(42, f.Invoke(instance, null));
    }

    /// <summary>
    /// A module that boxes an <c>i31</c> and reads it back survives the WASM-to-DLL path, exercising the
    /// <see cref="WebAssemblyI31"/> allocation and accessor under the persisted <c>NeutralizeType</c> mapping.
    /// </summary>
    [TestMethod]
    public void PersistedAssembly_I31_RoundTrip()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] }); // 0: () -> (i32)
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = "f", Index = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(-1),
                new RefI31(),                                      // (ref i31), payload masked to 0x7FFFFFFF
                new RefCast { HeapType = WebAssemblyHeapType.I31 }, // downcast via WebAssemblyI31
                new I31GetUnsigned(),                              // 2147483647
                new End(),
            ],
        });

        var (type, instance) = CompileLoadAndInstantiate(module, out _);

        var f = type.GetMethod("f");
        Assert.IsNotNull(f);
        Assert.AreEqual(2147483647, f.Invoke(instance, null));
    }
}
#endif

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly;

/// <summary>
/// Tests parsing and round-tripping of WASM 2.0 data segment kinds.
/// </summary>
[TestClass]
public class DataSegmentTests
{
    /// <summary>
    /// Tests that a kind-0 (active) data segment round-trips through Module.
    /// </summary>
    [TestMethod]
    public void DataSegment_Kind0_RoundTrips()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType());
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = "test" });
        module.Codes.Add(new FunctionBody { Code = [new Instructions.End()] });

        var seg = new Data();
        seg.InitializerExpression.Add(new Instructions.Int32Constant(0));
        seg.InitializerExpression.Add(new Instructions.End());
        seg.RawData.Add(1);
        seg.RawData.Add(2);
        module.Data.Add(seg);

        using var ms = new MemoryStream();
        module.WriteToBinary(ms);
        ms.Position = 0;
        var rt = Module.ReadFromBinary(ms);

        Assert.AreEqual(1, rt.Data.Count);
        Assert.AreEqual(0u, rt.Data[0].Kind);
        Assert.AreEqual(2, rt.Data[0].RawData.Count);
    }

    /// <summary>
    /// Tests that a kind-1 (passive) data segment round-trips through Module.
    /// </summary>
    [TestMethod]
    public void DataSegment_Kind1_RoundTrips()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType());
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = "test" });
        module.Codes.Add(new FunctionBody { Code = [new Instructions.End()] });

        var seg = new Data { Kind = 1 };
        seg.RawData.Add(99);
        module.Data.Add(seg);

        using var ms = new MemoryStream();
        module.WriteToBinary(ms);
        ms.Position = 0;
        var rt = Module.ReadFromBinary(ms);

        Assert.AreEqual(1, rt.Data.Count);
        Assert.AreEqual(1u, rt.Data[0].Kind);
        Assert.AreEqual(1, rt.Data[0].RawData.Count);
        Assert.AreEqual((byte)99, rt.Data[0].RawData[0]);
    }

    /// <summary>
    /// Tests that an active data segment initializer leaves exactly one i32 offset value on the stack.
    /// </summary>
    [TestMethod]
    public void DataSegment_ActiveInitializer_MustProduceExactlyOneI32()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType());
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = "test", Kind = ExternalKind.Function, Index = 0 });
        module.Codes.Add(new FunctionBody { Code = [new Instructions.End()] });

        var seg = new Data();
        seg.InitializerExpression.Add(new Instructions.Int32Constant(0));
        seg.InitializerExpression.Add(new Instructions.Int32Constant(1));
        seg.InitializerExpression.Add(new Instructions.End());
        module.Data.Add(seg);

        Assert.ThrowsException<StackSizeIncorrectException>(() => module.ToInstance<object>());
    }

    /// <summary>
    /// Tests that an active data segment with an explicit non-zero memory index is rejected.
    /// </summary>
    [TestMethod]
    public void DataSegment_ExplicitMemoryIndex_RequiresMemoryZero()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType());
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = "test", Kind = ExternalKind.Function, Index = 0 });
        module.Codes.Add(new FunctionBody { Code = [new Instructions.End()] });

        var seg = new Data { Kind = 2, MemoryIndex = 1 };
        seg.InitializerExpression.Add(new Instructions.Int32Constant(0));
        seg.InitializerExpression.Add(new Instructions.End());
        module.Data.Add(seg);

        Assert.ThrowsException<ModuleLoadException>(() => module.ToInstance<object>());
    }
}

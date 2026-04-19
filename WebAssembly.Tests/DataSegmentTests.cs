using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
}

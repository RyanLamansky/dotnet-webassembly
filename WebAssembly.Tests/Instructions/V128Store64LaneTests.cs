using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Store64Lane"/> instruction.</summary>
[TestClass]
public class V128Store64LaneTests
{
    /// <summary>Export for V128Store64Lane test.</summary>
    public abstract class V128Store64LaneExport
    {
        /// <summary>Stores lane 1 of a v128 to address 64, returns low 32 bits.</summary>
        public abstract int GetStoredLo();
        /// <summary>Returns high 32 bits.</summary>
        public abstract int GetStoredHi();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Store64LaneExport.GetStoredLo) });
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = nameof(V128Store64LaneExport.GetStoredHi), Index = 1 });

        // Lane 1 = bytes 8-15; set to LE encoding of 0x0102030405060708
        var bytes = new byte[16];
        bytes[8] = 0x08; bytes[9] = 0x07; bytes[10] = 0x06; bytes[11] = 0x05;
        bytes[12] = 0x04; bytes[13] = 0x03; bytes[14] = 0x02; bytes[15] = 0x01;

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(64),
                new V128Const { Value = bytes },
                new V128Store64Lane { LaneIndex = 1 },
                new Int32Constant(64),
                new Int32Load(),
                new End(),
            ],
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(68),
                new Int32Load(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.store64_lane writes the correct 64-bit lane to memory.</summary>
    [TestMethod]
    public void V128Store64Lane_WritesCorrectValue()
    {
        var compiled = BuildModule().ToInstance<V128Store64LaneExport>();
        Assert.AreEqual(0x05060708, compiled.Exports.GetStoredLo());
        Assert.AreEqual(0x01020304, compiled.Exports.GetStoredHi());
    }
}

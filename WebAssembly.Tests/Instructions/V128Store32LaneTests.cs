using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Store32Lane"/> instruction.</summary>
[TestClass]
public class V128Store32LaneTests
{
    /// <summary>Export for V128Store32Lane test.</summary>
    public abstract class V128Store32LaneExport
    {
        /// <summary>Stores lane 2 of a v128 to address 64, returns the 32-bit value stored.</summary>
        public abstract int GetStoredValue();
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
        module.Exports.Add(new Export { Name = nameof(V128Store32LaneExport.GetStoredValue) });

        // Lane 2 = bytes 8-11; set to 0x12345678 LE
        var bytes = new byte[16];
        bytes[8] = 0x78; bytes[9] = 0x56; bytes[10] = 0x34; bytes[11] = 0x12;

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(64),
                new V128Const { Value = bytes },
                new V128Store32Lane { LaneIndex = 2 },
                new Int32Constant(64),
                new Int32Load(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.store32_lane writes the correct 32-bit lane to memory.</summary>
    [TestMethod]
    public void V128Store32Lane_WritesCorrectValue()
    {
        var compiled = BuildModule().ToInstance<V128Store32LaneExport>();
        Assert.AreEqual(0x12345678, compiled.Exports.GetStoredValue());
    }
}

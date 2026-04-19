using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Store16Lane"/> instruction.</summary>
[TestClass]
public class V128Store16LaneTests
{
    /// <summary>Export for V128Store16Lane test.</summary>
    public abstract class V128Store16LaneExport
    {
        /// <summary>Stores lane 3 of a v128 to address 64, returns the 16-bit value stored.</summary>
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
        module.Exports.Add(new Export { Name = nameof(V128Store16LaneExport.GetStoredValue) });

        // Lane 3 = bytes 6-7
        var bytes = new byte[16];
        bytes[6] = 0x78; bytes[7] = 0x56;  // LE: 0x5678

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(64),
                new V128Const { Value = bytes },
                new V128Store16Lane { LaneIndex = 3 },
                new Int32Constant(64),
                new Int32Load16Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.store16_lane writes the correct 16-bit lane to memory.</summary>
    [TestMethod]
    public void V128Store16Lane_WritesCorrectValue()
    {
        var compiled = BuildModule().ToInstance<V128Store16LaneExport>();
        Assert.AreEqual(0x5678, compiled.Exports.GetStoredValue());
    }
}

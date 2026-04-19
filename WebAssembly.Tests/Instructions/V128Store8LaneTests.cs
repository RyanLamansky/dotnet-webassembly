using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Store8Lane"/> instruction.</summary>
[TestClass]
public class V128Store8LaneTests
{
    /// <summary>Export for V128Store8Lane test.</summary>
    public abstract class V128Store8LaneExport
    {
        /// <summary>Stores lane 5 of a v128 to address 64, returns the byte stored.</summary>
        public abstract int GetStoredByte();
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
        module.Exports.Add(new Export { Name = nameof(V128Store8LaneExport.GetStoredByte) });

        var bytes = new byte[16];
        bytes[5] = 0xCD;

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store8_lane: write lane 5 of the vector to address 64
                new Int32Constant(64),
                new V128Const { Value = bytes },
                new V128Store8Lane { LaneIndex = 5 },
                // read it back
                new Int32Constant(64),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.store8_lane writes the correct lane byte to memory.</summary>
    [TestMethod]
    public void V128Store8Lane_WritesCorrectByte()
    {
        var compiled = BuildModule().ToInstance<V128Store8LaneExport>();
        Assert.AreEqual(0xCD, compiled.Exports.GetStoredByte());
    }
}

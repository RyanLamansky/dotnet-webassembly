using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load8Lane"/> instruction.</summary>
[TestClass]
public class V128Load8LaneTests
{
    /// <summary>Export for V128Load8Lane test.</summary>
    public abstract class V128Load8LaneExport
    {
        /// <summary>Loads a byte from address 0 into lane 3 of a zero vector, returns byte at given offset.</summary>
        public abstract int GetByte(int offset);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load8LaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0xAB),
                new Int32Store8(),
                // v128.load8_lane: addr=0, vec=all-zeros, lane=3
                new Int32Constant(32),                      // dest for v128.store
                new Int32Constant(0),                       // addr
                new V128Const { Value = new byte[16] },    // all-zero vec
                new V128Load8Lane { LaneIndex = 3 },
                new V128Store(),
                new Int32Constant(32),
                new LocalGet(0),
                new Int32Add(),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.load8_lane loads into the correct lane.</summary>
    [TestMethod]
    public void V128Load8Lane_LoadsIntoSpecifiedLane()
    {
        var compiled = BuildModule().ToInstance<V128Load8LaneExport>();
        for (var i = 0; i < 16; i++)
        {
            var expected = i == 3 ? 0xAB : 0;
            Assert.AreEqual(expected, compiled.Exports.GetByte(i), $"Byte {i}");
        }
    }
}

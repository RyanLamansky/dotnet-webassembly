using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load32Lane"/> instruction.</summary>
[TestClass]
public class V128Load32LaneTests
{
    /// <summary>Export for V128Load32Lane test.</summary>
    public abstract class V128Load32LaneExport
    {
        /// <summary>Loads 4 bytes from address 0 into lane 1 of a zero vector, returns byte at given offset.</summary>
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
        module.Exports.Add(new Export { Name = nameof(V128Load32LaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0x01020304),
                new Int32Store(),
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Const { Value = new byte[16] },
                new V128Load32Lane { LaneIndex = 1 },
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

    /// <summary>Verifies that v128.load32_lane loads into the correct lane.</summary>
    [TestMethod]
    public void V128Load32Lane_LoadsIntoSpecifiedLane()
    {
        var compiled = BuildModule().ToInstance<V128Load32LaneExport>();
        // Lane 1 = bytes 4-7; 0x01020304 LE = [0x04, 0x03, 0x02, 0x01]
        var expected = new int[16];
        expected[4] = 0x04; expected[5] = 0x03; expected[6] = 0x02; expected[7] = 0x01;
        for (var i = 0; i < 16; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i}");
    }
}

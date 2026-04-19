using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load64Lane"/> instruction.</summary>
[TestClass]
public class V128Load64LaneTests
{
    /// <summary>Export for V128Load64Lane test.</summary>
    public abstract class V128Load64LaneExport
    {
        /// <summary>Loads 8 bytes from address 0 into lane 1 of a zero vector, returns byte at given offset.</summary>
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
        module.Exports.Add(new Export { Name = nameof(V128Load64LaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0x01020304),
                new Int32Store(),
                new Int32Constant(4),
                new Int32Constant(0x05060708),
                new Int32Store(),
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Const { Value = new byte[16] },
                new V128Load64Lane { LaneIndex = 1 },
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

    /// <summary>Verifies that v128.load64_lane loads into the correct lane.</summary>
    [TestMethod]
    public void V128Load64Lane_LoadsIntoSpecifiedLane()
    {
        var compiled = BuildModule().ToInstance<V128Load64LaneExport>();
        // Lane 1 = bytes 8-15; LE bytes: [0x04,0x03,0x02,0x01,0x08,0x07,0x06,0x05]
        var expected = new int[16];
        expected[8] = 0x04; expected[9] = 0x03; expected[10] = 0x02; expected[11] = 0x01;
        expected[12] = 0x08; expected[13] = 0x07; expected[14] = 0x06; expected[15] = 0x05;
        for (var i = 0; i < 16; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i}");
    }
}

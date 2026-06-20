using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load16Lane"/> instruction.</summary>
[TestClass]
public class V128Load16LaneTests
{
    /// <summary>Export for V128Load16Lane test.</summary>
    public abstract class V128Load16LaneExport
    {
        /// <summary>Loads 2 bytes from address 0 into lane 2 of a zero vector, returns byte at given offset.</summary>
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
        module.Exports.Add(new Export { Name = nameof(V128Load16LaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0x1234),
                new Int32Store16(),
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Const { Value = new byte[16] },
                new V128Load16Lane { LaneIndex = 2 },
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

    /// <summary>Verifies that v128.load16_lane loads into the correct lane.</summary>
    [TestMethod]
    public void V128Load16Lane_LoadsIntoSpecifiedLane()
    {
        var compiled = BuildModule().ToInstance<V128Load16LaneExport>();
        // Lane 2 occupies bytes 4-5; 0x1234 little-endian = [0x34, 0x12]
        for (var i = 0; i < 16; i++)
        {
            int expected = i == 4 ? 0x34 : i == 5 ? 0x12 : 0;
            Assert.AreEqual(expected, compiled.Exports.GetByte(i), $"Byte {i}");
        }
    }
}

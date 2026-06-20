using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load32Zero"/> instruction.</summary>
[TestClass]
public class V128Load32ZeroTests
{
    /// <summary>Export for V128Load32Zero test.</summary>
    public abstract class V128Load32ZeroExport
    {
        /// <summary>Returns byte at offset from the result vector stored at address 32.</summary>
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
        module.Exports.Add(new Export { Name = nameof(V128Load32ZeroExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store 0x01020304 at address 0
                new Int32Constant(0),
                new Int32Constant(0x01020304),
                new Int32Store(),
                // v128.load32_zero from address 0, store at 32
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Load32Zero(),
                new V128Store(),
                // return byte at 32+offset
                new Int32Constant(32),
                new LocalGet(0),
                new Int32Add(),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.load32_zero loads 4 bytes into low lane and zeros upper 12.</summary>
    [TestMethod]
    public void V128Load32Zero_LoadsLowLaneAndZerosUpper()
    {
        var compiled = BuildModule().ToInstance<V128Load32ZeroExport>();
        // Little-endian: 0x01020304 → bytes [0x04, 0x03, 0x02, 0x01, 0, 0, ...]
        Assert.AreEqual(0x04, compiled.Exports.GetByte(0));
        Assert.AreEqual(0x03, compiled.Exports.GetByte(1));
        Assert.AreEqual(0x02, compiled.Exports.GetByte(2));
        Assert.AreEqual(0x01, compiled.Exports.GetByte(3));
        for (var i = 4; i < 16; i++)
            Assert.AreEqual(0, compiled.Exports.GetByte(i), $"Byte {i} should be zero");
    }
}

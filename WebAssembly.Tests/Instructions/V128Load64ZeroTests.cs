using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load64Zero"/> instruction.</summary>
[TestClass]
public class V128Load64ZeroTests
{
    /// <summary>Export for V128Load64Zero test.</summary>
    public abstract class V128Load64ZeroExport
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
        module.Exports.Add(new Export { Name = nameof(V128Load64ZeroExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store 0x01020304 at address 0 (low 4 bytes)
                new Int32Constant(0),
                new Int32Constant(0x01020304),
                new Int32Store(),
                // store 0x05060708 at address 4 (high 4 bytes)
                new Int32Constant(4),
                new Int32Constant(0x05060708),
                new Int32Store(),
                // v128.load64_zero from address 0, store at 32
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Load64Zero(),
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

    /// <summary>Verifies that v128.load64_zero loads 8 bytes into low lane and zeros upper 8.</summary>
    [TestMethod]
    public void V128Load64Zero_LoadsLowLaneAndZerosUpper()
    {
        var compiled = BuildModule().ToInstance<V128Load64ZeroExport>();
        // Little-endian: bytes 0-3 = [0x04,0x03,0x02,0x01], bytes 4-7 = [0x08,0x07,0x06,0x05]
        Assert.AreEqual(0x04, compiled.Exports.GetByte(0));
        Assert.AreEqual(0x03, compiled.Exports.GetByte(1));
        Assert.AreEqual(0x02, compiled.Exports.GetByte(2));
        Assert.AreEqual(0x01, compiled.Exports.GetByte(3));
        Assert.AreEqual(0x08, compiled.Exports.GetByte(4));
        Assert.AreEqual(0x07, compiled.Exports.GetByte(5));
        Assert.AreEqual(0x06, compiled.Exports.GetByte(6));
        Assert.AreEqual(0x05, compiled.Exports.GetByte(7));
        for (var i = 8; i < 16; i++)
            Assert.AreEqual(0, compiled.Exports.GetByte(i), $"Byte {i} should be zero");
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Bitselect"/> instruction.</summary>
[TestClass]
public class V128BitselectTests
{
    /// <summary>Export for V128Bitselect test.</summary>
    public abstract class V128BitselectExport
    {
        /// <summary>Computes the instruction result and returns the byte at the given offset.</summary>
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(V128BitselectExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                // v1 = all 0xFF
                new V128Const { Value = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF] },
                // v2 = all 0x00
                new V128Const { Value = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00] },
                // mask = first 8 bytes 0xFF, last 8 bytes 0x00
                new V128Const { Value = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00] },
                new V128Bitselect(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies V128Bitselect produces correct results.</summary>
    [TestMethod]
    public void V128Bitselect_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<V128BitselectExport>();
        // First 8 bytes from v1 (0xFF), last 8 bytes from v2 (0x00)
        for (var i = 0; i < 8; i++)
            Assert.AreEqual(0xFF, compiled.Exports.GetByte(i), $"Byte {i} should be from v1");
        for (var i = 8; i < 16; i++)
            Assert.AreEqual(0x00, compiled.Exports.GetByte(i), $"Byte {i} should be from v2");
    }
}

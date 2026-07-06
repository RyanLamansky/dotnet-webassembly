using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Bitselect"/> instruction.</summary>
[TestClass]
public class V128BitselectTests
{
    /// <summary>Verifies V128Bitselect produces correct results.</summary>
    [TestMethod]
    public void V128Bitselect_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            // v1 = all 0xFF,
            new V128Const { Value = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF] },
            // v2 = all 0x00,
            new V128Const { Value = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00] },
            // mask = first 8 bytes 0xFF, last 8 bytes 0x00,
            new V128Const { Value = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00] },
            new V128Bitselect());
        // First 8 bytes from v1 (0xFF), last 8 bytes from v2 (0x00)
        for (var i = 0; i < 8; i++)
            Assert.AreEqual(0xFF, exports.GetByte(i), $"Byte {i} should be from v1");
        for (var i = 8; i < 16; i++)
            Assert.AreEqual(0x00, exports.GetByte(i), $"Byte {i} should be from v2");
    }
}

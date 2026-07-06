using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Xor"/> instruction.</summary>
[TestClass]
public class V128XorTests
{
    /// <summary>Verifies V128Xor produces correct results.</summary>
    [TestMethod]
    public void V128Xor_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new V128Const { Value = [15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15] },
            new V128Xor());
        int[] expected = [240, 240, 240, 240];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

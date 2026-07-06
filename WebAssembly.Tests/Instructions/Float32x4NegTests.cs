using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Neg"/> instruction.</summary>
[TestClass]
public class Float32x4NegTests
{
    /// <summary>Verifies Float32x4Neg produces correct results.</summary>
    [TestMethod]
    public void Float32x4Neg_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 192, 63, 0, 0, 192, 63, 0, 0, 192, 63, 0, 0, 192, 63] },
            new Float32x4Neg());
        int[] expected = [0, 0, 192, 191];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

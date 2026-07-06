using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Mul"/> instruction.</summary>
[TestClass]
public class Float64x2MulTests
{
    /// <summary>Verifies Float64x2Mul produces correct results.</summary>
    [TestMethod]
    public void Float64x2Mul_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 64] },
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 8, 64, 0, 0, 0, 0, 0, 0, 8, 64] },
            new Float64x2Mul());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

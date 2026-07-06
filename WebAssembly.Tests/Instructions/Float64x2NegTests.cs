using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Neg"/> instruction.</summary>
[TestClass]
public class Float64x2NegTests
{
    /// <summary>Verifies Float64x2Neg produces correct results.</summary>
    [TestMethod]
    public void Float64x2Neg_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 248, 63, 0, 0, 0, 0, 0, 0, 248, 63] },
            new Float64x2Neg());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

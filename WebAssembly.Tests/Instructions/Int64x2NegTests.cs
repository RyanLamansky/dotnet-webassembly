using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2Neg"/> instruction.</summary>
[TestClass]
public class Int64x2NegTests
{
    /// <summary>Verifies Int64x2Neg produces correct results.</summary>
    [TestMethod]
    public void Int64x2Neg_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2Neg());
        int[] expected = [255, 255, 255, 255];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

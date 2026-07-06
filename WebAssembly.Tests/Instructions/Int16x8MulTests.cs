using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Mul"/> instruction.</summary>
[TestClass]
public class Int16x8MulTests
{
    /// <summary>Verifies Int16x8Mul produces correct results.</summary>
    [TestMethod]
    public void Int16x8Mul_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0] },
            new V128Const { Value = [4, 0, 4, 0, 4, 0, 4, 0, 4, 0, 4, 0, 4, 0, 4, 0] },
            new Int16x8Mul());
        int[] expected = [12, 0, 12, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

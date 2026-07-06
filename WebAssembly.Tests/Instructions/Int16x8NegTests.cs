using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Neg"/> instruction.</summary>
[TestClass]
public class Int16x8NegTests
{
    /// <summary>Verifies Int16x8Neg produces correct results.</summary>
    [TestMethod]
    public void Int16x8Neg_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0] },
            new Int16x8Neg());
        int[] expected = [255, 255, 255, 255];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

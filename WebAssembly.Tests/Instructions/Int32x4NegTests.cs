using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4Neg"/> instruction.</summary>
[TestClass]
public class Int32x4NegTests
{
    /// <summary>Verifies Int32x4Neg produces correct results.</summary>
    [TestMethod]
    public void Int32x4Neg_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0] },
            new Int32x4Neg());
        int[] expected = [255, 255, 255, 255];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

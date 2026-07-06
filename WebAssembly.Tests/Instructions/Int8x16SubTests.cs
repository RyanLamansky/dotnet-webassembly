using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Sub"/> instruction.</summary>
[TestClass]
public class Int8x16SubTests
{
    /// <summary>Verifies Int8x16Sub produces correct results.</summary>
    [TestMethod]
    public void Int8x16Sub_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5] },
            new V128Const { Value = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3] },
            new Int8x16Sub());
        int[] expected = [2, 2, 2, 2];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

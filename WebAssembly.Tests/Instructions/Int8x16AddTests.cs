using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Add"/> instruction.</summary>
[TestClass]
public class Int8x16AddTests
{
    /// <summary>Verifies Int8x16Add produces correct results.</summary>
    [TestMethod]
    public void Int8x16Add_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1] },
            new V128Const { Value = [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2] },
            new Int8x16Add());
        int[] expected = [3, 3, 3, 3];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

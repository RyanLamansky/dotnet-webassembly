using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Add"/> instruction.</summary>
[TestClass]
public class Int16x8AddTests
{
    /// <summary>Verifies Int16x8Add produces correct results.</summary>
    [TestMethod]
    public void Int16x8Add_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0] },
            new V128Const { Value = [2, 0, 2, 0, 2, 0, 2, 0, 2, 0, 2, 0, 2, 0, 2, 0] },
            new Int16x8Add());
        int[] expected = [3, 0, 3, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

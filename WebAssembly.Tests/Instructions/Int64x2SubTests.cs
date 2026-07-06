using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2Sub"/> instruction.</summary>
[TestClass]
public class Int64x2SubTests
{
    /// <summary>Verifies Int64x2Sub produces correct results.</summary>
    [TestMethod]
    public void Int64x2Sub_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [5, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [3, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2Sub());
        int[] expected = [2, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

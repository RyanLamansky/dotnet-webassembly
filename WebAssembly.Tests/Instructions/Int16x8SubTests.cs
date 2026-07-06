using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Sub"/> instruction.</summary>
[TestClass]
public class Int16x8SubTests
{
    /// <summary>Verifies Int16x8Sub produces correct results.</summary>
    [TestMethod]
    public void Int16x8Sub_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0] },
            new V128Const { Value = [3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0] },
            new Int16x8Sub());
        int[] expected = [2, 0, 2, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

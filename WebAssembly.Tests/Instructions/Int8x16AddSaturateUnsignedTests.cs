using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16AddSaturateUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16AddSaturateUnsignedTests
{
    /// <summary>Verifies Int8x16AddSaturateUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16AddSaturateUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new V128Const { Value = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1] },
            new Int8x16AddSaturateUnsigned());
        int[] expected = [255, 255, 255, 255];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

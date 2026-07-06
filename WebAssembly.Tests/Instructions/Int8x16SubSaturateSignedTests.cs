using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16SubSaturateSigned"/> instruction.</summary>
[TestClass]
public class Int8x16SubSaturateSignedTests
{
    /// <summary>Verifies Int8x16SubSaturateSigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16SubSaturateSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128] },
            new V128Const { Value = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1] },
            new Int8x16SubSaturateSigned());
        int[] expected = [128, 128, 128, 128];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

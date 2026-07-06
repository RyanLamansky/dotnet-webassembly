using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8AddSaturateSigned"/> instruction.</summary>
[TestClass]
public class Int16x8AddSaturateSignedTests
{
    /// <summary>Verifies Int16x8AddSaturateSigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8AddSaturateSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127] },
            new V128Const { Value = [1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0] },
            new Int16x8AddSaturateSigned());
        int[] expected = [255, 127, 255, 127];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

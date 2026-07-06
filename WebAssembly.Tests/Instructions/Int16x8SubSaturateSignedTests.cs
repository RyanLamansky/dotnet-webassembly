using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8SubSaturateSigned"/> instruction.</summary>
[TestClass]
public class Int16x8SubSaturateSignedTests
{
    /// <summary>Verifies Int16x8SubSaturateSigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8SubSaturateSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128] },
            new V128Const { Value = [1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0] },
            new Int16x8SubSaturateSigned());
        int[] expected = [0, 128, 0, 128];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

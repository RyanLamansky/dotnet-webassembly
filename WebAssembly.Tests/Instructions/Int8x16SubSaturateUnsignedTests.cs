using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16SubSaturateUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16SubSaturateUnsignedTests
{
    /// <summary>Verifies Int8x16SubSaturateUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16SubSaturateUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1] },
            new Int8x16SubSaturateUnsigned());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

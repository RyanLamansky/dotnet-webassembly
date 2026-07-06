using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16MaxSigned"/> instruction.</summary>
[TestClass]
public class Int8x16MaxSignedTests
{
    /// <summary>Verifies Int8x16MaxSigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16MaxSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127] },
            new V128Const { Value = [128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128] },
            new Int8x16MaxSigned());
        int[] expected = [127, 127, 127, 127];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

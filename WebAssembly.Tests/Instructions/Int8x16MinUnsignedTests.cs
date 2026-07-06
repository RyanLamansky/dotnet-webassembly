using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16MinUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16MinUnsignedTests
{
    /// <summary>Verifies Int8x16MinUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16MinUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127] },
            new V128Const { Value = [128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128] },
            new Int8x16MinUnsigned());
        int[] expected = [127, 127, 127, 127];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

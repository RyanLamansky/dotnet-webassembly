using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8MinUnsigned"/> instruction.</summary>
[TestClass]
public class Int16x8MinUnsignedTests
{
    /// <summary>Verifies Int16x8MinUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8MinUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127] },
            new V128Const { Value = [0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128] },
            new Int16x8MinUnsigned());
        int[] expected = [255, 127, 255, 127];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

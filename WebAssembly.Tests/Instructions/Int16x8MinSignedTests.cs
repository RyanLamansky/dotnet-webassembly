using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8MinSigned"/> instruction.</summary>
[TestClass]
public class Int16x8MinSignedTests
{
    /// <summary>Verifies Int16x8MinSigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8MinSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127, 255, 127] },
            new V128Const { Value = [0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128, 0, 128] },
            new Int16x8MinSigned());
        int[] expected = [0, 128, 0, 128];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

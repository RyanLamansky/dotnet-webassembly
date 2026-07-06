using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4MaxUnsigned"/> instruction.</summary>
[TestClass]
public class Int32x4MaxUnsignedTests
{
    /// <summary>Verifies Int32x4MaxUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4MaxUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 127, 255, 255, 255, 127, 255, 255, 255, 127, 255, 255, 255, 127] },
            new V128Const { Value = [0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128] },
            new Int32x4MaxUnsigned());
        int[] expected = [0, 0, 0, 128];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

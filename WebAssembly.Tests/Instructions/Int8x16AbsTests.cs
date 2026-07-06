using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Abs"/> instruction.</summary>
[TestClass]
public class Int8x16AbsTests
{
    /// <summary>Verifies Int8x16Abs produces correct results.</summary>
    [TestMethod]
    public void Int8x16Abs_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new Int8x16Abs());
        int[] expected = [1, 1, 1, 1];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

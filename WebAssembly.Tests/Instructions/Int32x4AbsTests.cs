using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4Abs"/> instruction.</summary>
[TestClass]
public class Int32x4AbsTests
{
    /// <summary>Verifies Int32x4Abs produces correct results.</summary>
    [TestMethod]
    public void Int32x4Abs_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new Int32x4Abs());
        int[] expected = [1, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

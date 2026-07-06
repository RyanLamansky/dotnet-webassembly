using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Abs"/> instruction.</summary>
[TestClass]
public class Int16x8AbsTests
{
    /// <summary>Verifies Int16x8Abs produces correct results.</summary>
    [TestMethod]
    public void Int16x8Abs_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new Int16x8Abs());
        int[] expected = [1, 0, 1, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

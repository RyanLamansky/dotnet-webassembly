using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2Abs"/> instruction.</summary>
[TestClass]
public class Int64x2AbsTests
{
    /// <summary>Verifies Int64x2Abs produces correct results.</summary>
    [TestMethod]
    public void Int64x2Abs_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new Int64x2Abs());
        int[] expected = [1, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

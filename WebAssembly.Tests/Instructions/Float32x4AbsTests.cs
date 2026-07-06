using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Abs"/> instruction.</summary>
[TestClass]
public class Float32x4AbsTests
{
    /// <summary>Verifies Float32x4Abs produces correct results.</summary>
    [TestMethod]
    public void Float32x4Abs_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 192, 191] },
            new Float32x4Abs());
        int[] expected = [0, 0, 192, 63];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Pmax"/> instruction.</summary>
[TestClass]
public class Float32x4PmaxTests
{
    /// <summary>Verifies Float32x4Pmax produces correct results.</summary>
    [TestMethod]
    public void Float32x4Pmax_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new V128Const { Value = [0, 0, 0, 64, 0, 0, 0, 64, 0, 0, 0, 64, 0, 0, 0, 64] },
            new Float32x4Pmax());
        int[] expected = [0, 0, 0, 64];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

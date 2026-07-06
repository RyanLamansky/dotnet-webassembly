using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Max"/> instruction.</summary>
[TestClass]
public class Float64x2MaxTests
{
    /// <summary>Verifies Float64x2Max produces correct results.</summary>
    [TestMethod]
    public void Float64x2Max_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 240, 63, 0, 0, 0, 0, 0, 0, 240, 63] },
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 64] },
            new Float64x2Max());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

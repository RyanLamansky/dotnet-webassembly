using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Ceil"/> instruction.</summary>
[TestClass]
public class Float64x2CeilTests
{
    /// <summary>Verifies Float64x2Ceil produces correct results.</summary>
    [TestMethod]
    public void Float64x2Ceil_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [51, 51, 51, 51, 51, 51, 243, 63, 51, 51, 51, 51, 51, 51, 243, 63] },
            new Float64x2Ceil());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

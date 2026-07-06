using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Floor"/> instruction.</summary>
[TestClass]
public class Float64x2FloorTests
{
    /// <summary>Verifies Float64x2Floor produces correct results.</summary>
    [TestMethod]
    public void Float64x2Floor_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [205, 204, 204, 204, 204, 204, 252, 63, 205, 204, 204, 204, 204, 204, 252, 63] },
            new Float64x2Floor());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

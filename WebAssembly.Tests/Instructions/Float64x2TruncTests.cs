using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Trunc"/> instruction.</summary>
[TestClass]
public class Float64x2TruncTests
{
    /// <summary>Verifies Float64x2Trunc produces correct results.</summary>
    [TestMethod]
    public void Float64x2Trunc_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [102, 102, 102, 102, 102, 102, 254, 63, 102, 102, 102, 102, 102, 102, 254, 63] },
            new Float64x2Trunc());
        int[] expected = [0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

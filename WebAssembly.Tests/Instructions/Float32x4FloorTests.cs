using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Floor"/> instruction.</summary>
[TestClass]
public class Float32x4FloorTests
{
    /// <summary>Verifies Float32x4Floor produces correct results.</summary>
    [TestMethod]
    public void Float32x4Floor_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [102, 102, 230, 63, 102, 102, 230, 63, 102, 102, 230, 63, 102, 102, 230, 63] },
            new Float32x4Floor());
        int[] expected = [0, 0, 128, 63];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

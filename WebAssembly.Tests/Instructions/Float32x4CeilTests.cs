using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Ceil"/> instruction.</summary>
[TestClass]
public class Float32x4CeilTests
{
    /// <summary>Verifies Float32x4Ceil produces correct results.</summary>
    [TestMethod]
    public void Float32x4Ceil_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [154, 153, 153, 63, 154, 153, 153, 63, 154, 153, 153, 63, 154, 153, 153, 63] },
            new Float32x4Ceil());
        int[] expected = [0, 0, 0, 64];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

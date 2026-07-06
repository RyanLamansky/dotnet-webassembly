using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Sqrt"/> instruction.</summary>
[TestClass]
public class Float32x4SqrtTests
{
    /// <summary>Verifies Float32x4Sqrt produces correct results.</summary>
    [TestMethod]
    public void Float32x4Sqrt_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 64, 0, 0, 128, 64, 0, 0, 128, 64, 0, 0, 128, 64] },
            new Float32x4Sqrt());
        int[] expected = [0, 0, 0, 64];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

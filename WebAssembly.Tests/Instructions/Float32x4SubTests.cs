using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Sub"/> instruction.</summary>
[TestClass]
public class Float32x4SubTests
{
    /// <summary>Verifies Float32x4Sub produces correct results.</summary>
    [TestMethod]
    public void Float32x4Sub_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 160, 64, 0, 0, 160, 64, 0, 0, 160, 64, 0, 0, 160, 64] },
            new V128Const { Value = [0, 0, 0, 64, 0, 0, 0, 64, 0, 0, 0, 64, 0, 0, 0, 64] },
            new Float32x4Sub());
        int[] expected = [0, 0, 64, 64];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

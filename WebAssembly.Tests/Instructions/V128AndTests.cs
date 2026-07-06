using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128And"/> instruction.</summary>
[TestClass]
public class V128AndTests
{
    /// <summary>Verifies V128And produces correct results.</summary>
    [TestMethod]
    public void V128And_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255] },
            new V128Const { Value = [15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15] },
            new V128And());
        int[] expected = [15, 15, 15, 15];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

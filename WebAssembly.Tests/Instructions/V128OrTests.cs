using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Or"/> instruction.</summary>
[TestClass]
public class V128OrTests
{
    /// <summary>Verifies V128Or produces correct results.</summary>
    [TestMethod]
    public void V128Or_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15] },
            new V128Const { Value = [240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240] },
            new V128Or());
        int[] expected = [255, 255, 255, 255];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

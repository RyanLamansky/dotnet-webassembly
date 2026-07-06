using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Not"/> instruction.</summary>
[TestClass]
public class V128NotTests
{
    /// <summary>Verifies V128Not produces correct results.</summary>
    [TestMethod]
    public void V128Not_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240, 240] },
            new V128Not());
        int[] expected = [15, 15, 15, 15];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

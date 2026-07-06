using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Popcnt"/> instruction.</summary>
[TestClass]
public class Int8x16PopcntTests
{
    /// <summary>Verifies Int8x16Popcnt produces correct results.</summary>
    [TestMethod]
    public void Int8x16Popcnt_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16Popcnt());
        Assert.AreEqual(8, exports.GetByte(0));
    }
}

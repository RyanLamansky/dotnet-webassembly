using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4GreaterThan"/> instruction.</summary>
[TestClass]
public class Float32x4GreaterThanTests
{
    /// <summary>Verifies Float32x4GreaterThan produces correct results.</summary>
    [TestMethod]
    public void Float32x4GreaterThan_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 64, 64, 0, 0, 64, 64, 0, 0, 64, 64, 0, 0, 64, 64] },
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new Float32x4GreaterThan());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

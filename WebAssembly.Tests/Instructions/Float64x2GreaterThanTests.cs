using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2GreaterThan"/> instruction.</summary>
[TestClass]
public class Float64x2GreaterThanTests
{
    /// <summary>Verifies Float64x2GreaterThan produces correct results.</summary>
    [TestMethod]
    public void Float64x2GreaterThan_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 64] },
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 240, 63, 0, 0, 0, 0, 0, 0, 240, 63] },
            new Float64x2GreaterThan());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

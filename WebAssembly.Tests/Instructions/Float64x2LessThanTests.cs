using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2LessThan"/> instruction.</summary>
[TestClass]
public class Float64x2LessThanTests
{
    /// <summary>Verifies Float64x2LessThan produces correct results.</summary>
    [TestMethod]
    public void Float64x2LessThan_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 240, 63, 0, 0, 0, 0, 0, 0, 240, 63] },
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 64] },
            new Float64x2LessThan());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

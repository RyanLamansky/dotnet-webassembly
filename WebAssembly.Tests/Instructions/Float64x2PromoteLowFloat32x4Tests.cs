using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2PromoteLowFloat32x4"/> instruction.</summary>
[TestClass]
public class Float64x2PromoteLowFloat32x4Tests
{
    /// <summary>Verifies Float64x2PromoteLowFloat32x4 produces correct results.</summary>
    [TestMethod]
    public void Float64x2PromoteLowFloat32x4_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new Float64x2PromoteLowFloat32x4());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

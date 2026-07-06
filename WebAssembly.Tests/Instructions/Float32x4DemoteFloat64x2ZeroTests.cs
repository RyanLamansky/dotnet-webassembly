using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4DemoteFloat64x2Zero"/> instruction.</summary>
[TestClass]
public class Float32x4DemoteFloat64x2ZeroTests
{
    /// <summary>Verifies Float32x4DemoteFloat64x2Zero produces correct results.</summary>
    [TestMethod]
    public void Float32x4DemoteFloat64x2Zero_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 240, 63, 0, 0, 0, 0, 0, 0, 240, 63] },
            new Float32x4DemoteFloat64x2Zero());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

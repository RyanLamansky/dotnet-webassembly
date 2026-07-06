using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4TruncSatFloat64x2UnsignedZero"/> instruction.</summary>
[TestClass]
public class Int32x4TruncSatFloat64x2UnsignedZeroTests
{
    /// <summary>Verifies Int32x4TruncSatFloat64x2UnsignedZero produces correct results.</summary>
    [TestMethod]
    public void Int32x4TruncSatFloat64x2UnsignedZero_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 240, 63, 0, 0, 0, 0, 0, 0, 240, 63] },
            new Int32x4TruncSatFloat64x2UnsignedZero());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

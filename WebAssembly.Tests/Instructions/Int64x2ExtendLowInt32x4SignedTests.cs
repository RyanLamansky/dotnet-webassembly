using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2ExtendLowInt32x4Signed"/> instruction.</summary>
[TestClass]
public class Int64x2ExtendLowInt32x4SignedTests
{
    /// <summary>Verifies Int64x2ExtendLowInt32x4Signed produces correct results.</summary>
    [TestMethod]
    public void Int64x2ExtendLowInt32x4Signed_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2ExtendLowInt32x4Signed());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

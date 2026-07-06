using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2ExtendLowInt32x4Unsigned"/> instruction.</summary>
[TestClass]
public class Int64x2ExtendLowInt32x4UnsignedTests
{
    /// <summary>Verifies Int64x2ExtendLowInt32x4Unsigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2ExtendLowInt32x4Unsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2ExtendLowInt32x4Unsigned());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

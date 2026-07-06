using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8ExtendLowInt8x16Signed"/> instruction.</summary>
[TestClass]
public class Int16x8ExtendLowInt8x16SignedTests
{
    /// <summary>Verifies Int16x8ExtendLowInt8x16Signed produces correct results.</summary>
    [TestMethod]
    public void Int16x8ExtendLowInt8x16Signed_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int16x8ExtendLowInt8x16Signed());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

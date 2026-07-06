using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8ExtmulLowInt8x16Unsigned"/> instruction.</summary>
[TestClass]
public class Int16x8ExtmulLowInt8x16UnsignedTests
{
    /// <summary>Verifies Int16x8ExtmulLowInt8x16Unsigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8ExtmulLowInt8x16Unsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int16x8ExtmulLowInt8x16Unsigned());
        Assert.AreEqual(6, exports.GetByte(0));
    }
}

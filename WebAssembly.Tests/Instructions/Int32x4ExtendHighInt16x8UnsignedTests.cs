using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4ExtendHighInt16x8Unsigned"/> instruction.</summary>
[TestClass]
public class Int32x4ExtendHighInt16x8UnsignedTests
{
    /// <summary>Verifies Int32x4ExtendHighInt16x8Unsigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4ExtendHighInt16x8Unsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0] },
            new Int32x4ExtendHighInt16x8Unsigned());
        Assert.AreEqual(2, exports.GetByte(0));
    }
}

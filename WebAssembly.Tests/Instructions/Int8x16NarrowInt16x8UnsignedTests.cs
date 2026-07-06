using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16NarrowInt16x8Unsigned"/> instruction.</summary>
[TestClass]
public class Int8x16NarrowInt16x8UnsignedTests
{
    /// <summary>Verifies Int8x16NarrowInt16x8Unsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16NarrowInt16x8Unsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16NarrowInt16x8Unsigned());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16LessThanUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16LessThanUnsignedTests
{
    /// <summary>Verifies Int8x16LessThanUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16LessThanUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16LessThanUnsigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

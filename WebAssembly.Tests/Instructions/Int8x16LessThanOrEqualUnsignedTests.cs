using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16LessThanOrEqualUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16LessThanOrEqualUnsignedTests
{
    /// <summary>Verifies Int8x16LessThanOrEqualUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16LessThanOrEqualUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16LessThanOrEqualUnsigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

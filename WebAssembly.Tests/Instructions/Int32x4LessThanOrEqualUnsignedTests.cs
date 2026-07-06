using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4LessThanOrEqualUnsigned"/> instruction.</summary>
[TestClass]
public class Int32x4LessThanOrEqualUnsignedTests
{
    /// <summary>Verifies Int32x4LessThanOrEqualUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4LessThanOrEqualUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32x4LessThanOrEqualUnsigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

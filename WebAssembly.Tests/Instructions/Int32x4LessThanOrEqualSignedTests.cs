using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4LessThanOrEqualSigned"/> instruction.</summary>
[TestClass]
public class Int32x4LessThanOrEqualSignedTests
{
    /// <summary>Verifies Int32x4LessThanOrEqualSigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4LessThanOrEqualSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32x4LessThanOrEqualSigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

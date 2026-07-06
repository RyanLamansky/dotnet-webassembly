using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4LessThanSigned"/> instruction.</summary>
[TestClass]
public class Int32x4LessThanSignedTests
{
    /// <summary>Verifies Int32x4LessThanSigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4LessThanSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32x4LessThanSigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2LessThanSigned"/> instruction.</summary>
[TestClass]
public class Int64x2LessThanSignedTests
{
    /// <summary>Verifies Int64x2LessThanSigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2LessThanSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2LessThanSigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

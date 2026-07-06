using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2GreaterThanSigned"/> instruction.</summary>
[TestClass]
public class Int64x2GreaterThanSignedTests
{
    /// <summary>Verifies Int64x2GreaterThanSigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2GreaterThanSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2GreaterThanSigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

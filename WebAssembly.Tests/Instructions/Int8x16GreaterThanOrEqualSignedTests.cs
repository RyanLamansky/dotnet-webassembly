using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16GreaterThanOrEqualSigned"/> instruction.</summary>
[TestClass]
public class Int8x16GreaterThanOrEqualSignedTests
{
    /// <summary>Verifies Int8x16GreaterThanOrEqualSigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16GreaterThanOrEqualSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16GreaterThanOrEqualSigned());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

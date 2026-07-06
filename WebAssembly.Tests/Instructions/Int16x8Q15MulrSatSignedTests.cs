using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Q15MulrSatSigned"/> instruction.</summary>
[TestClass]
public class Int16x8Q15MulrSatSignedTests
{
    /// <summary>Verifies Int16x8Q15MulrSatSigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8Q15MulrSatSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int16x8Q15MulrSatSigned());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16ShiftRightSigned"/> instruction.</summary>
[TestClass]
public class Int8x16ShiftRightSignedTests
{
    /// <summary>Verifies Int8x16ShiftRightSigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16ShiftRightSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int8x16ShiftRightSigned());
        Assert.AreEqual(192, exports.GetByte(0));
    }
}

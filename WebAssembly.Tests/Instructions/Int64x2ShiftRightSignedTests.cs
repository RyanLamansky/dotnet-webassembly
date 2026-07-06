using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2ShiftRightSigned"/> instruction.</summary>
[TestClass]
public class Int64x2ShiftRightSignedTests
{
    /// <summary>Verifies Int64x2ShiftRightSigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2ShiftRightSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int64x2ShiftRightSigned());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

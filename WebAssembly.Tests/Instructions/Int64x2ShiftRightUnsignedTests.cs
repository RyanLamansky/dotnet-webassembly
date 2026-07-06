using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2ShiftRightUnsigned"/> instruction.</summary>
[TestClass]
public class Int64x2ShiftRightUnsignedTests
{
    /// <summary>Verifies Int64x2ShiftRightUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2ShiftRightUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int64x2ShiftRightUnsigned());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

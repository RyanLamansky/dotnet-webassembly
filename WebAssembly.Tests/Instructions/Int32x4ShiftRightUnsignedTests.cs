using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4ShiftRightUnsigned"/> instruction.</summary>
[TestClass]
public class Int32x4ShiftRightUnsignedTests
{
    /// <summary>Verifies Int32x4ShiftRightUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4ShiftRightUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int32x4ShiftRightUnsigned());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

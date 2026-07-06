using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16ShiftRightUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16ShiftRightUnsignedTests
{
    /// <summary>Verifies Int8x16ShiftRightUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16ShiftRightUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int8x16ShiftRightUnsigned());
        Assert.AreEqual(64, exports.GetByte(0));
    }
}

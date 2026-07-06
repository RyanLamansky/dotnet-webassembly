using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8ShiftRightSigned"/> instruction.</summary>
[TestClass]
public class Int16x8ShiftRightSignedTests
{
    /// <summary>Verifies Int16x8ShiftRightSigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8ShiftRightSigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int16x8ShiftRightSigned());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2ShiftLeft"/> instruction.</summary>
[TestClass]
public class Int64x2ShiftLeftTests
{
    /// <summary>Verifies Int64x2ShiftLeft produces correct results.</summary>
    [TestMethod]
    public void Int64x2ShiftLeft_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int64x2ShiftLeft());
        Assert.AreEqual(2, exports.GetByte(0));
    }
}

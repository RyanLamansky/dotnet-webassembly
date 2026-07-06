using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16ShiftLeft"/> instruction.</summary>
[TestClass]
public class Int8x16ShiftLeftTests
{
    /// <summary>Verifies Int8x16ShiftLeft produces correct results.</summary>
    [TestMethod]
    public void Int8x16ShiftLeft_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32Constant(1),
            new Int8x16ShiftLeft());
        Assert.AreEqual(2, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2NotEqual"/> instruction.</summary>
[TestClass]
public class Int64x2NotEqualTests
{
    /// <summary>Verifies Int64x2NotEqual produces correct results.</summary>
    [TestMethod]
    public void Int64x2NotEqual_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2NotEqual());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4NotEqual"/> instruction.</summary>
[TestClass]
public class Int32x4NotEqualTests
{
    /// <summary>Verifies Int32x4NotEqual produces correct results.</summary>
    [TestMethod]
    public void Int32x4NotEqual_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int32x4NotEqual());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

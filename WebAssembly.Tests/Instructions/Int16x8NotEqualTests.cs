using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8NotEqual"/> instruction.</summary>
[TestClass]
public class Int16x8NotEqualTests
{
    /// <summary>Verifies Int16x8NotEqual produces correct results.</summary>
    [TestMethod]
    public void Int16x8NotEqual_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int16x8NotEqual());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

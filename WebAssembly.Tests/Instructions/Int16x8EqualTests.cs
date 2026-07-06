using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Equal"/> instruction.</summary>
[TestClass]
public class Int16x8EqualTests
{
    /// <summary>Verifies Int16x8Equal produces correct results.</summary>
    [TestMethod]
    public void Int16x8Equal_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int16x8Equal());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

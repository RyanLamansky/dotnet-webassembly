using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Equal"/> instruction.</summary>
[TestClass]
public class Int8x16EqualTests
{
    /// <summary>Verifies Int8x16Equal produces correct results.</summary>
    [TestMethod]
    public void Int8x16Equal_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16Equal());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

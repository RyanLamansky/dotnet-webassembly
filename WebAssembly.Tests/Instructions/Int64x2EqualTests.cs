using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2Equal"/> instruction.</summary>
[TestClass]
public class Int64x2EqualTests
{
    /// <summary>Verifies Int64x2Equal produces correct results.</summary>
    [TestMethod]
    public void Int64x2Equal_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int64x2Equal());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

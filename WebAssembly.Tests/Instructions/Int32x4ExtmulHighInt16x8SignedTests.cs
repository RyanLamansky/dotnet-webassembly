using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4ExtmulHighInt16x8Signed"/> instruction.</summary>
[TestClass]
public class Int32x4ExtmulHighInt16x8SignedTests
{
    /// <summary>Verifies Int32x4ExtmulHighInt16x8Signed produces correct results.</summary>
    [TestMethod]
    public void Int32x4ExtmulHighInt16x8Signed_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0] },
            new Int32x4ExtmulHighInt16x8Signed());
        Assert.AreEqual(6, exports.GetByte(0));
    }
}

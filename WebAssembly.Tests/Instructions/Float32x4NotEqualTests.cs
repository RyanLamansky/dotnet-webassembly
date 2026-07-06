using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4NotEqual"/> instruction.</summary>
[TestClass]
public class Float32x4NotEqualTests
{
    /// <summary>Verifies Float32x4NotEqual produces correct results.</summary>
    [TestMethod]
    public void Float32x4NotEqual_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new V128Const { Value = [0, 0, 64, 64, 0, 0, 64, 64, 0, 0, 64, 64, 0, 0, 64, 64] },
            new Float32x4NotEqual());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4LessThanOrEqual"/> instruction.</summary>
[TestClass]
public class Float32x4LessThanOrEqualTests
{
    /// <summary>Verifies Float32x4LessThanOrEqual produces correct results.</summary>
    [TestMethod]
    public void Float32x4LessThanOrEqual_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new Float32x4LessThanOrEqual());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

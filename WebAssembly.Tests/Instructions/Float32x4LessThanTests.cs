using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4LessThan"/> instruction.</summary>
[TestClass]
public class Float32x4LessThanTests
{
    /// <summary>Verifies Float32x4LessThan produces correct results.</summary>
    [TestMethod]
    public void Float32x4LessThan_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new V128Const { Value = [0, 0, 64, 64, 0, 0, 64, 64, 0, 0, 64, 64, 0, 0, 64, 64] },
            new Float32x4LessThan());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

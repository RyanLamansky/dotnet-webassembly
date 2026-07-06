using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Equal"/> instruction.</summary>
[TestClass]
public class Float32x4EqualTests
{
    /// <summary>Verifies Float32x4Equal produces correct results.</summary>
    [TestMethod]
    public void Float32x4Equal_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new Float32x4Equal());
        Assert.AreEqual(255, exports.GetByte(0));
    }
}

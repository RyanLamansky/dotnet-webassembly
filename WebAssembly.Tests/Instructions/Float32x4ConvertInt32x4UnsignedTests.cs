using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4ConvertInt32x4Unsigned"/> instruction.</summary>
[TestClass]
public class Float32x4ConvertInt32x4UnsignedTests
{
    /// <summary>Verifies Float32x4ConvertInt32x4Unsigned produces correct results.</summary>
    [TestMethod]
    public void Float32x4ConvertInt32x4Unsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0] },
            new Float32x4ConvertInt32x4Unsigned());
        Assert.AreEqual(0, exports.GetByte(0));
    }
}

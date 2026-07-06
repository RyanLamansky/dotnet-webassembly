using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4TruncSatFloat32x4Unsigned"/> instruction.</summary>
[TestClass]
public class Int32x4TruncSatFloat32x4UnsignedTests
{
    /// <summary>Verifies Int32x4TruncSatFloat32x4Unsigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4TruncSatFloat32x4Unsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new Int32x4TruncSatFloat32x4Unsigned());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

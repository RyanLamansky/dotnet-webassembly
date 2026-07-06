using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4TruncSatFloat32x4Signed"/> instruction.</summary>
[TestClass]
public class Int32x4TruncSatFloat32x4SignedTests
{
    /// <summary>Verifies Int32x4TruncSatFloat32x4Signed produces correct results.</summary>
    [TestMethod]
    public void Int32x4TruncSatFloat32x4Signed_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63] },
            new Int32x4TruncSatFloat32x4Signed());
        Assert.AreEqual(1, exports.GetByte(0));
    }
}

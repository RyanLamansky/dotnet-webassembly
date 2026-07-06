using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Swizzle"/> instruction.</summary>
[TestClass]
public class Int8x16SwizzleTests
{
    /// <summary>Verifies Int8x16Swizzle produces correct results.</summary>
    [TestMethod]
    public void Int8x16Swizzle_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            // a = [10,20,30,40, 50,60,70,80, 90,100,110,120, 130,140,150,160],
            new V128Const { Value = [10,20,30,40, 50,60,70,80, 90,100,110,120, 130,140,150,160] },
            // indices = [0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0] → all first byte (10),
            new V128Const { Value = [0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0] },
            new Int8x16Swizzle());
        Assert.AreEqual(10, exports.GetByte(0), "Byte 0 mismatch");
    }
}

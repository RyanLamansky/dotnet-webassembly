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

    /// <summary>Verifies out-of-range indices (>= 16) select zero, per i8x16.swizzle semantics.</summary>
    [TestMethod]
    public void Int8x16Swizzle_OutOfRangeSelectsZero()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160] },
            // In-range indices in the first four lanes, out-of-range (16, 17, 128, 255, ...) in the rest.
            new V128Const { Value = [0, 15, 1, 14, 16, 17, 128, 255, 16, 16, 16, 16, 16, 16, 16, 16] },
            new Int8x16Swizzle());
        int[] expected = [10, 160, 20, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], exports.GetByte(i), $"Byte {i} mismatch");
    }
}

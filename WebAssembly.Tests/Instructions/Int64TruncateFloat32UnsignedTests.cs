using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int64TruncateFloat32Unsigned"/> instruction.
/// </summary>
[TestClass]
public class Int64TruncateFloat32UnsignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int64TruncateFloat32Unsigned"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int64TruncateUnsignedFloat32_Compiled()
    {
        var exports = ConversionTestBase<float, long>.CreateInstance(
            new LocalGet(0),
            new Int64TruncateFloat32Unsigned(),
            new End());

        Assert.AreEqual(0L, exports.Test(0f));
        Assert.AreEqual(1L, exports.Test(1.5f));
        Assert.AreEqual((long)123445678901234f, exports.Test(123445678901234f));
        // 2^63 fits the unsigned range but not the signed range; the 64-bit result is its two's-complement pattern.
        Assert.AreEqual(unchecked((long)9223372036854775808u), exports.Test(9223372036854775808f));

        // Values below the unsigned range (< 0) and above it (>= 2^64) trap.
        Assert.ThrowsException<System.OverflowException>(() => exports.Test(-1.5f));
        Assert.ThrowsException<System.OverflowException>(() => exports.Test(1234456789012345678901234567890f));
    }
}

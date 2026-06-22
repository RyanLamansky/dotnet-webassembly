using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int64TruncateFloat64Unsigned"/> instruction.
/// </summary>
[TestClass]
public class Int64TruncateFloat64UnsignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int64TruncateFloat64Unsigned"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int64TruncateUnsignedFloat64_Compiled()
    {
        var exports = ConversionTestBase<double, long>.CreateInstance(
            new LocalGet(0),
            new Int64TruncateFloat64Unsigned(),
            new End());

        Assert.AreEqual(0L, exports.Test(0.0));
        Assert.AreEqual(1L, exports.Test(1.5));
        Assert.AreEqual(123445678901234L, exports.Test(123445678901234.0));
        // 2^63 fits the unsigned range but not the signed range; the 64-bit result is its two's-complement pattern.
        Assert.AreEqual(unchecked((long)9223372036854775808u), exports.Test(9223372036854775808.0));

        // Values below the unsigned range (< 0) and above it (>= 2^64) trap.
        Assert.ThrowsExactly<System.OverflowException>(() => exports.Test(-1.5));
        Assert.ThrowsExactly<System.OverflowException>(() => exports.Test(1234456789012345678901234567890.0));
    }
}

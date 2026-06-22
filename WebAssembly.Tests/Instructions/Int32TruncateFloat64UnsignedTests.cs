using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int32TruncateFloat64Unsigned"/> instruction.
/// </summary>
[TestClass]
public class Int32TruncateFloat64UnsignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int32TruncateFloat64Unsigned"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int32TruncateUnsignedFloat64_Compiled()
    {
        var exports = ConversionTestBase<double, int>.CreateInstance(
            new LocalGet(0),
            new Int32TruncateFloat64Unsigned(),
            new End());

        Assert.AreEqual(0, exports.Test(0.0));
        Assert.AreEqual(1, exports.Test(1.5));
        // 2^31 fits the unsigned range but not the signed range; the 32-bit result is its two's-complement pattern.
        Assert.AreEqual(unchecked((int)2147483648u), exports.Test(2147483648.0));

        // Values below the unsigned range (< 0) and above it (>= 2^32) trap.
        Assert.ThrowsExactly<System.OverflowException>(() => exports.Test(-1.5));
        Assert.ThrowsExactly<System.OverflowException>(() => exports.Test(123445678901234.0));
    }
}

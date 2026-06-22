using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int32TruncateFloat32Unsigned"/> instruction.
/// </summary>
[TestClass]
public class Int32TruncateFloat32UnsignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int32TruncateFloat32Unsigned"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int32TruncateUnsignedFloat32_Compiled()
    {
        var exports = ConversionTestBase<float, int>.CreateInstance(
            new LocalGet(0),
            new Int32TruncateFloat32Unsigned(),
            new End());

        Assert.AreEqual(0, exports.Test(0f));
        Assert.AreEqual(1, exports.Test(1.5f));
        // 2^31 fits the unsigned range but not the signed range; the 32-bit result is its two's-complement pattern.
        Assert.AreEqual(unchecked((int)2147483648u), exports.Test(2147483648f));

        // Values below the unsigned range (< 0) and above it (>= 2^32) trap.
        Assert.ThrowsExactly<System.OverflowException>(() => exports.Test(-1.5f));
        Assert.ThrowsExactly<System.OverflowException>(() => exports.Test(123445678901234f));
    }
}

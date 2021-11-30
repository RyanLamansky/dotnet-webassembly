using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int64RotateRight"/> instruction.
/// </summary>
[TestClass]
public class Int64RotateRightTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int64RotateRight"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int64RotateRight_Compiled()
    {
        var exports = CompilerTestBase2<long>.CreateInstance(
            new LocalGet(0),
            new LocalGet(1),
            new Int64RotateRight(),
            new End());

        //Test cases from https://github.com/WebAssembly/spec/blob/f1b89dfaf379060c7e35eb90b7daeb14d4ade3f7/test/core/i64.wast
        Assert.AreEqual(unchecked((long)0x8000000000000000), exports.Test(1, 1));
        Assert.AreEqual(1, exports.Test(1, 0));
        Assert.AreEqual(-1, exports.Test(-1, 1));
        Assert.AreEqual(1, exports.Test(1, 64));
        Assert.AreEqual(0x55e6cc3b01234567, exports.Test(unchecked((long)0xabcd987602468ace), 1));
        Assert.AreEqual(0x0fe000000dc00000, exports.Test(unchecked((long)0xfe000000dc000000), 4));
        Assert.AreEqual(0x6891a77ab3c04d5e, exports.Test(unchecked((long)0xabcd1234ef567809), 53));
        Assert.AreEqual(0x57a2469deacf0139, exports.Test(unchecked((long)0xabd1234ef567809c), 63));
        Assert.AreEqual(0x6891a77ab3c04d5e, exports.Test(unchecked((long)0xabcd1234ef567809), 0xf5));
        Assert.AreEqual(unchecked((long)0x94a77ab3c04d5e6b), exports.Test(unchecked((long)0xabcd7294ef567809), unchecked((long)0xffffffffffffffed)));
        Assert.AreEqual(0x57a2469deacf0139, exports.Test(unchecked((long)0xabd1234ef567809c), unchecked((long)0x800000000000003f)));
        Assert.AreEqual(2, exports.Test(1, 63));
        Assert.AreEqual(1, exports.Test(unchecked((long)0x8000000000000000), 63));
    }
}

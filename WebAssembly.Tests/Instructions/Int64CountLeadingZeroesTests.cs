using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int64CountLeadingZeroes"/> instruction.
/// </summary>
[TestClass]
public class Int64CountLeadingZeroesTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int64CountLeadingZeroes"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int64CountLeadingZeroes_Compiled()
    {
        if (!System.Environment.Is64BitProcess)
            Assert.Inconclusive("32-bit .NET doesn't support the 64-bit bit shifts used by the count leading zeroes helper logic.");

        var exports = CompilerTestBase<long>.CreateInstance(
            new LocalGet(0),
            new Int64CountLeadingZeroes(),
            new End());

        //Test cases from https://github.com/WebAssembly/spec/blob/f1b89dfaf379060c7e35eb90b7daeb14d4ade3f7/test/core/i64.wast
        Assert.AreEqual(0, exports.Test(unchecked((long)0xffffffffffffffff)));
        Assert.AreEqual(64, exports.Test(0));
        Assert.AreEqual(48, exports.Test(0x00008000));
        Assert.AreEqual(56, exports.Test(0xff));
        Assert.AreEqual(0, exports.Test(unchecked((long)0x8000000000000000)));
        Assert.AreEqual(63, exports.Test(1));
        Assert.AreEqual(62, exports.Test(2));
        Assert.AreEqual(1, exports.Test(0x7fffffffffffffff));
    }
}

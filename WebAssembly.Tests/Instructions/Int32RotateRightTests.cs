using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32RotateRight"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32RotateRightTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32RotateRight"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32RotateRight_Compiled()
        {
            var exports = CompilerTestBase2<int>.CreateInstance(
                new LocalGet(0),
                new LocalGet(1),
                new Int32RotateRight(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/f1b89dfaf379060c7e35eb90b7daeb14d4ade3f7/test/core/i32.wast
            Assert.AreEqual(unchecked((int)0x80000000), exports.Test(1, 1));
            Assert.AreEqual(1, exports.Test(1, 0));
            Assert.AreEqual(-1, exports.Test(-1, 1));
            Assert.AreEqual(1, exports.Test(1, 32));
            Assert.AreEqual(0x7f806600, exports.Test(unchecked((int)0xff00cc00), 1));
            Assert.AreEqual(0x00008000, exports.Test(0x00080000, 4));
            Assert.AreEqual(0x1d860e97, exports.Test(unchecked((int)0xb0c1d2e3), 5));
            Assert.AreEqual(0x00000400, exports.Test(0x00008000, 37));
            Assert.AreEqual(0x1d860e97, exports.Test(unchecked((int)0xb0c1d2e3), 0xff05));
            Assert.AreEqual(unchecked((int)0xe6fbb4d5), exports.Test(0x769abcdf, unchecked((int)0xffffffed)));
            Assert.AreEqual(unchecked((int)0xe6fbb4d5), exports.Test(0x769abcdf, unchecked((int)0x8000000d)));
            Assert.AreEqual(2, exports.Test(1, 31));
            Assert.AreEqual(1, exports.Test(unchecked((int)0x80000000), 31));
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32CountOneBits"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32CountOneBitsTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32CountOneBits"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32CountOneBits_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new LocalGet(0),
                new Int32CountOneBits(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/f1b89dfaf379060c7e35eb90b7daeb14d4ade3f7/test/core/i32.wast
            Assert.AreEqual(32, exports.Test(-1));
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(1, exports.Test(0x00008000));
            Assert.AreEqual(2, exports.Test(unchecked((int)0x80008000)));
            Assert.AreEqual(31, exports.Test(0x7fffffff));
            Assert.AreEqual(16, exports.Test(unchecked((int)0xAAAAAAAA)));
            Assert.AreEqual(16, exports.Test(0x55555555));
            Assert.AreEqual(24, exports.Test(unchecked((int)0xDEADBEEF)));
        }
    }
}
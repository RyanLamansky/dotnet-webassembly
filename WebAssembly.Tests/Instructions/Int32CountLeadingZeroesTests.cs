using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32CountLeadingZeroes"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32CountLeadingZeroesTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32CountLeadingZeroes"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32CountLeadingZeroes_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new GetLocal(0),
                new Int32CountLeadingZeroes(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/f1b89dfaf379060c7e35eb90b7daeb14d4ade3f7/test/core/i32.wast
            Assert.AreEqual(0, exports.Test(unchecked((int)0xffffffff)));
            Assert.AreEqual(32, exports.Test(0));
            Assert.AreEqual(16, exports.Test(0x00008000));
            Assert.AreEqual(24, exports.Test(0xff));
            Assert.AreEqual(0, exports.Test(unchecked((int)0x80000000)));
            Assert.AreEqual(31, exports.Test(1));
            Assert.AreEqual(30, exports.Test(2));
            Assert.AreEqual(1, exports.Test(0x7fffffff));
        }
    }
}
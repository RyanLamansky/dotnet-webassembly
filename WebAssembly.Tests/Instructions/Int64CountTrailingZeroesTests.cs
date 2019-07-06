using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64CountTrailingZeroes"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64CountTrailingZeroesTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64CountTrailingZeroes"/> instruction.
        /// </summary>
        [TestMethod]
        public void IInt64CountTrailingZeroes_Compiled()
        {
            var exports = CompilerTestBase<long>.CreateInstance(
                new LocalGet(0),
                new Int64CountTrailingZeroes(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/f1b89dfaf379060c7e35eb90b7daeb14d4ade3f7/test/core/i64.wast
            Assert.AreEqual(0, exports.Test(-1));
            Assert.AreEqual(64, exports.Test(0));
            Assert.AreEqual(15, exports.Test(0x00008000));
            Assert.AreEqual(16, exports.Test(0x00010000));
            Assert.AreEqual(63, exports.Test(unchecked((long)0x8000000000000000)));
            Assert.AreEqual(0, exports.Test(0x7fffffffffffffff));
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64Extend32Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64Extend32SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Extend32Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Extend32Signed_Compiled()
        {
            var exports = ConversionTestBase<long, long>.CreateInstance(
                new LocalGet(0),
                new Int64Extend32Signed(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/i64.wast
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(32767, exports.Test(0x7fff));
            Assert.AreEqual(32768, exports.Test(0x8000));
            Assert.AreEqual(65535, exports.Test(0xffff));
            Assert.AreEqual(0x7fffffff, exports.Test(0x7fffffff));
            Assert.AreEqual(-0x80000000, exports.Test(0x80000000));
            Assert.AreEqual(-1, exports.Test(0xffffffff));
            Assert.AreEqual(0, exports.Test(0x0123456700000000));
            Assert.AreEqual(-0x80000000, exports.Test(unchecked((long)0xfedcba9880000000)));
            Assert.AreEqual(-1, exports.Test(-1));
        }
    }
}
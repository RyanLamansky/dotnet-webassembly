using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32Extend8Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32Extend8SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32Extend8Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32Extend8Signed_Compiled()
        {
            var exports = ConversionTestBase<int, int>.CreateInstance(
                new LocalGet(0),
                new Int32Extend8Signed(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/i32.wast
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(127, exports.Test(0x7f));
            Assert.AreEqual(-128, exports.Test(0x80));
            Assert.AreEqual(-1, exports.Test(0xff));
            Assert.AreEqual(0, exports.Test(0x01234500));
            Assert.AreEqual(-0x80, exports.Test(unchecked((int)0xfedcba80)));
            Assert.AreEqual(-1, exports.Test(-1));
        }
    }
}
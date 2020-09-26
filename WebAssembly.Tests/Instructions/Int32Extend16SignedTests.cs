using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32Extend16Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32Extend16SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32Extend16Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32Extend16Signed_Compiled()
        {
            var exports = ConversionTestBase<int, int>.CreateInstance(
                new LocalGet(0),
                new Int32Extend16Signed(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/i32.wast
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(32767, exports.Test(0x7fff));
            Assert.AreEqual(-32768, exports.Test(0x8000));
            Assert.AreEqual(-1, exports.Test(0xffff));
            Assert.AreEqual(0, exports.Test(0x01230000));
            Assert.AreEqual(-0x8000, exports.Test(unchecked((int)0xfedc8000)));
            Assert.AreEqual(-1, exports.Test(-1));
        }
    }
}
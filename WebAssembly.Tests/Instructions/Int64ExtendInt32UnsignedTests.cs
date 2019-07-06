using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64ExtendInt32Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64ExtendInt32UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64ExtendInt32Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64ExtendUnsignedInt32_Compiled()
        {
            var exports = ConversionTestBase<int, long>.CreateInstance(
                new LocalGet(0),
                new Int64ExtendInt32Unsigned(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/c4774b47d326e4114f96232f1389a555639d7348/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(10000, exports.Test(10000));
            Assert.AreEqual(0x00000000ffffd8f0, exports.Test(-10000));
            Assert.AreEqual(0xffffffff, exports.Test(-1));
            Assert.AreEqual(0x000000007fffffff, exports.Test(0x7fffffff));
            Assert.AreEqual(unchecked((long)0x0000000080000000), exports.Test(unchecked((int)0x80000000)));
        }
    }
}
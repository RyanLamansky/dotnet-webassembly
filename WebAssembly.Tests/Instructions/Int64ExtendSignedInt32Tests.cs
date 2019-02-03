using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64ExtendSignedInt32"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64ExtendSignedInt32Tests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64ExtendSignedInt32"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64ExtendSignedInt32_Compiled()
        {
            var exports = ConversionTestBase<int, long>.CreateInstance(
                new GetLocal(0),
                new Int64ExtendSignedInt32(),
                new End());

            //Test cases from https://github.com/WebAssembly/spec/blob/c4774b47d326e4114f96232f1389a555639d7348/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(10000, exports.Test(10000));
            Assert.AreEqual(-10000, exports.Test(-10000));
            Assert.AreEqual(-1, exports.Test(-1));
            Assert.AreEqual(0x000000007fffffff, exports.Test(0x7fffffff));
            Assert.AreEqual(unchecked((long)0xffffffff80000000), exports.Test(unchecked((int)0x80000000)));
        }
    }
}
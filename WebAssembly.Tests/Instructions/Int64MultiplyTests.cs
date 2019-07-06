using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64Multiply"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64MultiplyTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Multiply"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Multiply_Compiled()
        {
            const int comparand = 0xF;

            var exports = CompilerTestBase<long>.CreateInstance(
                new LocalGet(0),
                new Int64Constant(comparand),
                new Int64Multiply(),
                new End());

            foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value * comparand, exports.Test(value));
        }
    }
}
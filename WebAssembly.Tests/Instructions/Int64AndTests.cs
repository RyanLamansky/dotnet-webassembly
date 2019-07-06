using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64And"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64AndTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64And"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64And_Compiled()
        {
            const int and = 0xF;

            var exports = CompilerTestBase<long>.CreateInstance(
                new LocalGet(0),
                new Int64Constant(and),
                new Int64And(),
                new End());

            foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value & and, exports.Test(value));
        }
    }
}
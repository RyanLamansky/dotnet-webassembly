using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32Multiply"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32MultiplyTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32Multiply"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32Multiply_Compiled()
        {
            const int comparand = 0xF;

            var exports = CompilerTestBase<int>.CreateInstance(
                new LocalGet(0),
                new Int32Constant(comparand),
                new Int32Multiply(),
                new End());

            foreach (var value in new[] { 0x00, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value * comparand, exports.Test(value));
        }
    }
}
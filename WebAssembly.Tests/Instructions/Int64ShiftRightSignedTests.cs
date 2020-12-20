using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64ShiftRightSigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64ShiftRightSignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64ShiftRightSigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64ShiftRightSigned_Compiled()
        {
            const int amount = 0xF;

            var exports = CompilerTestBase<long>.CreateInstance(
                new LocalGet(0),
                new Int64Constant(amount),
                new Int64ShiftRightSigned(),
                new End());

            foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value >> amount, exports.Test(value));
        }
    }
}
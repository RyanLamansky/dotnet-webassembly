using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64ShiftRightUnsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64ShiftRightUnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64ShiftRightUnsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64ShiftRightUnsigned_Compiled()
        {
            const int amount = 0xF;

            var exports = CompilerTestBase<long>.CreateInstance(
                new LocalGet(0),
                new Int64Constant(amount),
                new Int64ShiftRightUnsigned(),
                new End());

            foreach (var value in new ulong[] { 0x00, 0x01, 0x02, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value >> amount, (ulong)exports.Test((long)value));
        }
    }
}
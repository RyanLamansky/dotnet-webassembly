using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32ShiftRightUnsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32ShiftRightUnsignedests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32ShiftRightUnsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32ShiftRightUnsigned_Compiled()
        {
            const int amount = 0xF;

            var exports = CompilerTestBase<int>.CreateInstance(
                new GetLocal(0),
                new Int32Constant(amount),
                new Int32ShiftRightUnsigned(),
                new End());

            foreach (var value in new uint[] { 0x00, 0x01, 0x02, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value >> amount, (uint)exports.Test((int)value));
        }
    }
}
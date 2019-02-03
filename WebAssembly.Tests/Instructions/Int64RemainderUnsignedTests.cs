using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64RemainderUnsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64RemainderUnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64RemainderUnsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64RemainderUnsigned_Compiled()
        {
            const uint divisor = 0xF;

            var exports = CompilerTestBase<long>.CreateInstance(
                new GetLocal(0),
                new Int64Constant(divisor),
                new Int64RemainderUnsigned(),
                new End());

            foreach (var value in new ulong[] { 0x00, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value % divisor, (ulong)exports.Test((long)value));
        }
    }
}
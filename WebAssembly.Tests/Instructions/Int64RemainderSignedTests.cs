using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64RemainderSigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64RemainderSignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64RemainderSigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64RemainderSigned_Compiled()
        {
            const int divisor = 0xF;

            var exports = CompilerTestBase<long>.CreateInstance(
                new GetLocal(0),
                new Int64Constant(divisor),
                new Int64RemainderSigned(),
                new End());

            foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
                Assert.AreEqual(value % divisor, exports.Test(value));
        }
    }
}
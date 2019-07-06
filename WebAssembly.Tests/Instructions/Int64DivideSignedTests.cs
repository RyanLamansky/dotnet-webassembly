using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64DivideSigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64DivideSignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64DivideSigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64DivideSigned_Compiled()
        {
            const int divisor = 2;

            var exports = CompilerTestBase<long>.CreateInstance(
                new LocalGet(0),
                new Int64Constant(divisor),
                new Int64DivideSigned(),
                new End());

            foreach (var value in new long[] { 0, 1, 2, 3, 4, 5, })
                Assert.AreEqual(value / divisor, exports.Test(value));
        }
    }
}
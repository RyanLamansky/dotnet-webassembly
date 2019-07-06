using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32DivideSigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32DivideSignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32DivideSigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32DivideSigned_Compiled()
        {
            const int divisor = 2;

            var exports = CompilerTestBase<int>.CreateInstance(
                new LocalGet(0),
                new Int32Constant(divisor),
                new Int32DivideSigned(),
                new End());

            foreach (var value in new[] { 0, 1, 2, 3, 4, 5, })
                Assert.AreEqual(value / divisor, exports.Test(value));
        }
    }
}
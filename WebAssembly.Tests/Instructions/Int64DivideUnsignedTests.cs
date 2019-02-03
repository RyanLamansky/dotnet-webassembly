using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64DivideUnsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64DivideUnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Add"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64DivideUnsigned_Compiled()
        {
            const uint divisor = 2;

            var exports = CompilerTestBase<long>.CreateInstance(
                new GetLocal(0),
                new Int64Constant(divisor),
                new Int64DivideUnsigned(),
                new End());

            foreach (var value in new ulong[] { 0, 1, 2, 3, 4, 5, })
                Assert.AreEqual(value / divisor, (ulong)exports.Test((long)value));
        }
    }
}
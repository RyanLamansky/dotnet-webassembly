using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32EqualZero"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32EqualZeroTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32EqualZero"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32EqualZero_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new GetLocal(0),
                new Int32EqualZero(),
                new End());

            foreach (var value in Samples.Int32)
                Assert.AreEqual(value == 0, exports.Test(value) != 0);
        }
    }
}
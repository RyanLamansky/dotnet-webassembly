using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Negate"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64NegateTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Negate"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Negate_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new GetLocal(0),
                new Float64Negate(),
                new End());

            foreach (var value in Samples.Double)
                Assert.AreEqual(-value, exports.Test(value));
        }
    }
}
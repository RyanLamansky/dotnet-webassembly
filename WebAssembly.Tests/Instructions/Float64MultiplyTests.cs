using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Multiply"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64MultiplyTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Multiply"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Multiply_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new LocalGet(0),
                new Float64Constant(3),
                new Float64Multiply(),
                new End());

            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(9, exports.Test(3));
            Assert.AreEqual(-6, exports.Test(-2));
        }
    }
}
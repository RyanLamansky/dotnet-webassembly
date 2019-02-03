using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Divide"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64DivideTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Divide"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Divide_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new GetLocal(0),
                new Float64Constant(3),
                new Float64Divide(),
                new End());

            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(3, exports.Test(9));
            Assert.AreEqual(-2, exports.Test(-6));
        }
    }
}
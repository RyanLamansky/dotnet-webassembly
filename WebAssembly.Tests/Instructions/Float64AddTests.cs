using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Add"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64AddTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Add"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Add_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new LocalGet(0),
                new Float64Constant(1),
                new Float64Add(),
                new End());

            Assert.AreEqual(1, exports.Test(0));
            Assert.AreEqual(6, exports.Test(5));
        }
    }
}
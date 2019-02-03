using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="TeeLocal"/> instruction.
    /// </summary>
    [TestClass]
    public class TeeLocalTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="TeeLocal"/> instruction.
        /// </summary>
        [TestMethod]
        public void TeeLocal_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new GetLocal(0),
                new TeeLocal(0),
                new End());

            Assert.AreEqual(3, exports.Test(3));
            Assert.AreEqual(-1, exports.Test(-1));
        }
    }
}
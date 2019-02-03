using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="SetLocal"/> instruction.
    /// </summary>
    [TestClass]
    public class SetLocalTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="SetLocal"/> instruction.
        /// </summary>
        [TestMethod]
        public void SetLocal_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new GetLocal(0),
                new Int32Constant(1),
                new Int32Add(),
                new SetLocal(0),
                new GetLocal(0),
                new Int32Constant(1),
                new Int32Add(),
                new End());

            Assert.AreEqual(2, exports.Test(0));
            Assert.AreEqual(3, exports.Test(1));
        }
    }
}
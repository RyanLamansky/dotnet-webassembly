using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="BranchIf"/> instruction.
    /// </summary>
    [TestClass]
    public class BranchIfTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="BranchIf"/> instruction.
        /// </summary>
        [TestMethod]
        public void BranchIf_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new Block(BlockType.Empty),
                new GetLocal(0),
                new BranchIf(0),
                new Int32Constant(2),
                new Return(),
                new End(),
                new Int32Constant(1),
                new End());

            Assert.AreEqual(2, exports.Test(0));
            Assert.AreEqual(1, exports.Test(1));
        }
    }
}
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
                new LocalGet(0),
                new BranchIf(0),
                new Int32Constant(2),
                new Return(),
                new End(),
                new Int32Constant(1),
                new End());

            Assert.AreEqual(2, exports.Test(0));
            Assert.AreEqual(1, exports.Test(1));
        }

        /// <summary>
        /// Tests compilation of the <see cref="BranchIf"/> and <see cref="Loop"/> instructions that yields a value with no way for it to end.
        /// </summary>
        [TestMethod]
        public void BranchIf_LoopInfiniteWithValue()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Int32Constant(3),
                new Int32Constant(1),
                new BranchIf(),
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="BranchIf"/> and <see cref="Loop"/> instructions that yields a value.
        /// </summary>
        [TestMethod]
        public void BranchIf_LoopBreakWithValue()
        {
            var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                    new Int32Constant(3),
                    new Int32Constant(0),
                    new BranchIf(),
                    new End(),
                    new End());

            Assert.AreEqual<int>(3, exports.Test());
        }
    }
}
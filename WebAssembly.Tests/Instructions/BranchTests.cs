using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Branch"/> instruction.
    /// </summary>
    [TestClass]
    public class BranchTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Branch"/> instruction.
        /// </summary>
        [TestMethod]
        public void Branch_Compiled()
        {
            var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
                WebAssemblyValueType.Int32,
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new End(),
                new Block(BlockType.Empty),
                new Branch(1),
                new End(),
                new End(),
                new Int32Constant(2),
                new Return(),
                new End(),
                new Int32Constant(1),
                new End());

            Assert.AreEqual<int>(2, exports.Test());
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Branch"/> and <see cref="Loop"/> instructions with appropriate stack tracking.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Branch_LoopStackTracking()
        {
            var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
                WebAssemblyValueType.Int32,
                new Int32Constant(1),
                new Block(BlockType.Int32),
                new Loop(BlockType.Int32), // The stack outside the loop should not be carried inside.
                new Int32Constant(2),
                new Branch(1), // Break out of the loop by jumping to the outer block.
                new End(),
                new End(),
                new Int32Add(), // The pre-loop stack is restored here with the loop results at the top.
                new End());

            Assert.AreEqual<int>(3, exports.Test());
        }

        /// <summary>
        /// Tests compilation of the <see cref="Branch"/> and <see cref="Loop"/> instructions with no way for it to end.
        /// </summary>
        [TestMethod]
        public void Branch_LoopInfinite()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                null,
                new Loop(),
                new Branch(),
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation of the <see cref="Branch"/> and <see cref="Loop"/> instructions that yields a value with no way for it to end.
        /// </summary>
        [TestMethod]
        public void Branch_LoopInfiniteWithValue()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Branch(), // Continuing a loop should disregard the stack.
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation of the <see cref="Branch"/> and <see cref="Loop"/> instructions that yields a value and has a discarded value with no way for it to end.
        /// </summary>
        [TestMethod]
        public void Branch_LoopInfiniteWithValueAndDiscard()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Int32Constant(1),
                new Branch(), // Continuing a loop should disregard the stack.
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation of the <see cref="Branch"/> and <see cref="Loop"/> instructions that yields a value and has discarded values with no way for it to end.
        /// </summary>
        [TestMethod]
        public void Branch_LoopInfiniteWithValueAndDiscards()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Int32Constant(1),
                new Int32Constant(2),
                new Branch(), // Continuing a loop should disregard the stack.
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation of the <see cref="Branch"/> and <see cref="Loop"/> instructions that yields a value and has a discarded value of the wrong type with no way for it to end.
        /// </summary>
        [TestMethod]
        public void Branch_LoopInfiniteWithValueAndDiscardedWrongType()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Float64Constant(1),
                new Branch(), // Continuing a loop should disregard the stack.
                new End(),
                new End());
        }
    }
}
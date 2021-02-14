using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="BranchTable"/> instruction.
    /// </summary>
    [TestClass]
    public class BranchTableTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="BranchTable"/> instruction.
        /// </summary>
        [TestMethod]
        public void BranchTable_Compiled()
        {
            var exports = AssemblyBuilder.CreateInstance<CompilerTestBase<int>>("Test",
                WebAssemblyValueType.Int32,
                 new[]
                 {
                     WebAssemblyValueType.Int32
                 },
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new LocalGet(0),
                new BranchTable(0, 0, 1, 2, 3),
                new End(),
                new Int32Constant(1),
                new Return(),
                new End(),
                new Int32Constant(2),
                new Return(),
                new End(),
                new Int32Constant(3),
                new Return(),
                new End(),
                new Int32Constant(4),
                new Return(),
                new End());

            Assert.AreEqual(1, exports.Test(-1));
            Assert.AreEqual(1, exports.Test(0));
            Assert.AreEqual(2, exports.Test(1));
            Assert.AreEqual(3, exports.Test(2));
            Assert.AreEqual(4, exports.Test(3));
            Assert.AreEqual(1, exports.Test(4));
            Assert.AreEqual(1, exports.Test(5));
        }

        /// <summary>
        /// Tests the <see cref="BranchTable.Equals(Instruction)"/> and <see cref="BranchTable.GetHashCode()"/> methods.
        /// </summary>
        [TestMethod]
        public void BranchTable_Equals()
        {
            TestUtility.CreateInstances<BranchTable>(out var a, out var b);

            a.DefaultLabel = 2;
            b.DefaultLabel = 1;
            TestUtility.AreNotEqual(a, b);
            b.DefaultLabel = 2;
            TestUtility.AreEqual(a, b);

            b.Labels = new uint[] { };
            TestUtility.AreEqual(a, b);
            b.Labels = new uint[] { 1 };
            TestUtility.AreNotEqual(a, b);
            a.Labels = new uint[] { 1 };
            TestUtility.AreEqual(a, b);
            a.Labels = new uint[] { 1, 2 };
            TestUtility.AreNotEqual(a, b);
        }

        /// <summary>
        /// Tests compilation of the <see cref="BranchTable"/> and <see cref="Loop"/> instructions that yields a value with no way for it to end.
        /// </summary>
        [TestMethod]
        public void BranchTable_LoopInfiniteWithValue()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Int32Constant(7),
                new Int32Constant(0),
                new BranchTable(0, 0),
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation of the <see cref="BranchTable"/> and <see cref="Loop"/> instructions that yields a value.
        /// </summary>
        [TestMethod]
        public void BranchTable_LoopBreakWithValue()
        {
            var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
                WebAssemblyValueType.Int32,
                new Block(BlockType.Int32),
                new Loop(BlockType.Int32),
                new Int32Constant(7),
                new Int32Constant(0),
                new BranchTable(1, 1),
                new End(),
                new End(),
                new End());

            Assert.AreEqual<int>(7, exports.Test());
        }

        /// <summary>
        /// Tests compilation of the <see cref="BranchTable"/> and <see cref="Loop"/> instructions that yields a value and has a discarded value with no way for it to end.
        /// </summary>
        [TestMethod]
        public void BranchTable_LoopInfiniteWithValueAndDiscard()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Int32Constant(6),
                new Int32Constant(7),
                new Int32Constant(0),
                new BranchTable(0, 0),
                new End(),
                new End());
        }

        /// <summary>
        /// Tests compilation of the <see cref="BranchTable"/> and <see cref="Loop"/> instructions that yields a value and has a discarded value of the wrong type with no way for it to end.
        /// </summary>
        [TestMethod]
        public void BranchTable_LoopInfiniteWithValueAndDiscardedWrongType()
        {
            _ = AssemblyBuilder.CreateInstance<object>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Float64Constant(1),
                new Int32Constant(0),
                new BranchTable(0, 0),
                new End(),
                new End());
        }
    }
}
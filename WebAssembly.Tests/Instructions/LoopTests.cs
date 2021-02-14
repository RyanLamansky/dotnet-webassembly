using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Loop"/> instruction.
    /// </summary>
    [TestClass]
    public class LoopTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Loop"/> instruction.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Loop_Compiled()
        {
            var exports = CompilerTestBase2<int>.CreateInstance(
                new Block(BlockType.Empty),
                new Loop(BlockType.Empty),

                new LocalGet(0),
                new Int32Constant(1),
                new Int32Add(),
                new LocalSet(0),

                new LocalGet(1),
                new Int32Constant(1),
                new Int32Add(),
                new LocalSet(1),

                new LocalGet(1),
                new If(BlockType.Empty),
                new Branch(2),
                new Else(),
                new Branch(1),
                new End(), //if

                new End(), //loop
                new End(), //block
                new LocalGet(0),
                new End());

            Assert.AreEqual(11, exports.Test(10, -2));
            Assert.AreEqual(12, exports.Test(10, -1));
            Assert.AreEqual(11, exports.Test(10, 0));
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Loop"/> instruction that yields a value.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Branch_LoopValue()
        {
            var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
                WebAssemblyValueType.Int32,
                new Loop(BlockType.Int32),
                new Int32Constant(7),
                new End(),
                new End());

            Assert.AreEqual<int>(7, exports.Test());
        }
    }
}
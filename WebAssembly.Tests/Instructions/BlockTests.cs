using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Block"/> instruction.
    /// </summary>
    [TestClass]
    public class BlockTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Block"/> instruction.
        /// </summary>
        [TestMethod]
        public void Block_Compiled()
        {
            var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
                ValueType.Int32,
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new End(),
                new Block(BlockType.Empty),
                new Block(BlockType.Empty),
                new End(),
                new End(),
                new End(),
                new Int32Constant(6),
                new End());

            Assert.AreEqual<int>(6, exports.Test());
        }
    }
}
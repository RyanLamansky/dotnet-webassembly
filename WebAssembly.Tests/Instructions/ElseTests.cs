using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Else"/> instruction.
    /// </summary>
    [TestClass]
    public class ElseTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Else"/> instruction.
        /// </summary>
        [TestMethod]
        public void Else_Compiled()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new LocalGet(0),
                new If(),
                new Int32Constant(3),
                new Return(),
                new Else(),
                new Int32Constant(2),
                new Return(),
                new End(),
                new Int32Constant(1),
                new End());

            Assert.AreEqual(2, exports.Test(0));
            Assert.AreEqual(3, exports.Test(1));
        }


        /// <summary>
        /// Tests compilation and execution of the <see cref="Else"/> instruction.
        /// </summary>
        [TestMethod]
        public void Else_Compiled_CarriedValue()
        {
            var exports = CompilerTestBase<int>.CreateInstance(
                new LocalGet(0),
                new If(BlockType.Int32),
                new Int32Constant(1),
                new Else(),
                new Int32Constant(2),
                new End(),
                new End());

            Assert.AreEqual(2, exports.Test(0));
            Assert.AreEqual(1, exports.Test(1));
            Assert.AreEqual(1, exports.Test(2));
        }
    }
}
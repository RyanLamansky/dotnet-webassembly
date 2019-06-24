using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Drop"/> instruction.
    /// </summary>
    [TestClass]
    public class DropTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Drop"/> instruction.
        /// </summary>
        [TestMethod]
        public void Drop_Compiled()
        {
            Assert.AreEqual<int>(1, AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32, new Int32Constant(1), new Int32Constant(2), new Drop(), new End()).Test());

            var stackTooSmall = Assert.ThrowsException<StackTooSmallException>(() => AssemblyBuilder.CreateInstance<dynamic>("Test", null, new Drop(), new End()).Test());
            Assert.AreEqual(OpCode.Drop, stackTooSmall.OpCode);
            Assert.AreEqual(1, stackTooSmall.Minimum);
            Assert.AreEqual(0, stackTooSmall.Actual);
        }
    }
}
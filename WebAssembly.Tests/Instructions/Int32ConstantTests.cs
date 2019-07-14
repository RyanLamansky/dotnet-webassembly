using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32Constant"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32ConstantTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32Constant"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32Constant_Compiled()
        {
            foreach (var sample in new[]
            {
                -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, //Dedicated .NET Opcodes
				byte.MaxValue,
                short.MinValue,
                short.MaxValue,
                ushort.MaxValue,
                int.MinValue,
                int.MaxValue,
            })
            {
                Assert.AreEqual<int>(sample, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
                    new Int32Constant(sample),
                    new End()
                    ).Test());
            }
        }
    }
}
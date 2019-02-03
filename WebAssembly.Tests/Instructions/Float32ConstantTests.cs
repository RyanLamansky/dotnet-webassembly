using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32Constant"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32ConstantTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32Constant"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32Constant_Compiled()
        {
            foreach (var sample in new float[]
            {
                -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, //Dedicated .NET Opcodes
				byte.MaxValue,
                short.MinValue,
                short.MaxValue,
                ushort.MaxValue,
                int.MinValue,
                int.MaxValue,
                uint.MaxValue,
                long.MinValue,
                long.MaxValue,
                (float)Math.PI,
                -(float)Math.PI,
            })
            {
                Assert.AreEqual<float>(sample, AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Float32,
                    new Float32Constant(sample),
                    new End()
                    ).Test());
            }
        }
    }
}
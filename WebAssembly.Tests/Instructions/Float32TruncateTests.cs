using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32Truncate"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32TruncateTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32Truncate"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32Truncate_Compiled()
        {
            var exports = CompilerTestBase<float>.CreateInstance(
                new LocalGet(0),
                new Float32Truncate(),
                new End());

            foreach (var value in Samples.Single)
                Assert.AreEqual((float)Math.Truncate(value), exports.Test(value));
        }
    }
}
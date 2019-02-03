using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32Absolute"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32AbsoluteTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32Absolute"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32Absolute_Compiled()
        {
            var exports = CompilerTestBase<float>.CreateInstance(
                new GetLocal(0),
                new Float32Absolute(),
                new End());

            foreach (var value in new[] { 1f, -1f, -(float)Math.PI, (float)Math.PI })
                Assert.AreEqual(Math.Abs(value), exports.Test(value));
        }
    }
}
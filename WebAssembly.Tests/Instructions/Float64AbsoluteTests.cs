using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Absolute"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64AbsoluteTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Absolute"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Absolute_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new GetLocal(0),
                new Float64Absolute(),
                new End());

            foreach (var value in new[] { 1f, -1f, -Math.PI, Math.PI })
                Assert.AreEqual(Math.Abs(value), exports.Test(value));
        }
    }
}
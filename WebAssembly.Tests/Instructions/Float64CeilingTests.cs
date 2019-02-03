using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Ceiling"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64CeilingTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Ceiling"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Ceiling_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new GetLocal(0),
                new Float64Ceiling(),
                new End());

            foreach (var value in new[] { 1f, -1f, -Math.PI, Math.PI })
                Assert.AreEqual(Math.Ceiling(value), exports.Test(value));
        }
    }
}
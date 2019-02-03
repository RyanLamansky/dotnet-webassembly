using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Nearest"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64NearestTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Nearest"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Nearest_Compiled()
        {
            var exports = CompilerTestBase<double>.CreateInstance(
                new GetLocal(0),
                new Float64Nearest(),
                new End());

            foreach (var value in new[] { 1f, -1f, -Math.PI, Math.PI })
                Assert.AreEqual(Math.Round(value, MidpointRounding.ToEven), exports.Test(value));
        }
    }
}
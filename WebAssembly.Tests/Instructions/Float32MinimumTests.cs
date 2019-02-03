using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32Minimum"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32MinimumTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32Minimum"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32Minimum_Compiled()
        {
            var exports = CompilerTestBase2<float>.CreateInstance(
                new GetLocal(0),
                new GetLocal(1),
                new Float32Minimum(),
                new End());

            var values = new[]
            {
                0f,
                1f,
                -1f,
                -(float)Math.PI,
                (float)Math.PI,
                float.NaN,
                float.NegativeInfinity,
                float.PositiveInfinity,
                float.Epsilon,
                -float.Epsilon,
            };

            foreach (var comparand in values)
            {
                foreach (var value in values)
                    Assert.AreEqual(Math.Min(comparand, value), exports.Test(comparand, value));

                foreach (var value in values)
                    Assert.AreEqual(Math.Min(value, comparand), exports.Test(value, comparand));
            }
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64GreaterThan"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64GreaterThanTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64GreaterThan"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64GreaterThan_Compiled()
        {
            var exports = ComparisonTestBase<double>.CreateInstance(
                new LocalGet(0),
                new LocalGet(1),
                new Float64GreaterThan(),
                new End());

            var values = new[]
            {
                0.0,
                1.0,
                -1.0,
                -Math.PI,
                Math.PI,
                double.NaN,
                double.NegativeInfinity,
                double.PositiveInfinity,
                double.Epsilon,
                -double.Epsilon,
            };

            foreach (var comparand in values)
            {
                foreach (var value in values)
                    Assert.AreEqual(comparand > value, exports.Test(comparand, value) != 0);

                foreach (var value in values)
                    Assert.AreEqual(value > comparand, exports.Test(value, comparand) != 0);
            }
        }
    }
}
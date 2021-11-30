using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float32GreaterThan"/> instruction.
/// </summary>
[TestClass]
public class Float32GreaterThanTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float32GreaterThan"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float32GreaterThan_Compiled()
    {
        var exports = ComparisonTestBase<float>.CreateInstance(
            new LocalGet(0),
            new LocalGet(1),
            new Float32GreaterThan(),
            new End());

        var values = new[]
        {
                0.0f,
                1.0f,
                -1.0f,
                -(float)Math.PI,
                (float)Math.PI,
                (float)double.NaN,
                (float)double.NegativeInfinity,
                (float)double.PositiveInfinity,
                (float)double.Epsilon,
                -(float)double.Epsilon,
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

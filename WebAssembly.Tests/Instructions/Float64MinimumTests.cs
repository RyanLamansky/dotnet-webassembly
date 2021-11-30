using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float64Minimum"/> instruction.
/// </summary>
[TestClass]
public class Float64MinimumTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float64Minimum"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float64Minimum_Compiled()
    {
        var exports = CompilerTestBase2<double>.CreateInstance(
            new LocalGet(0),
            new LocalGet(1),
            new Float64Minimum(),
            new End());

        var values = new[]
        {
                0d,
                1d,
                -1d,
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
                Assert.AreEqual(Math.Min(comparand, value), exports.Test(comparand, value));

            foreach (var value in values)
                Assert.AreEqual(Math.Min(value, comparand), exports.Test(value, comparand));
        }
    }
}

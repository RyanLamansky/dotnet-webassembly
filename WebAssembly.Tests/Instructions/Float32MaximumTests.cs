using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float32Maximum"/> instruction.
/// </summary>
[TestClass]
public class Float32MaximumTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float32Maximum"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float32Maximum_Compiled()
    {
        var exports = CompilerTestBase2<float>.CreateInstance(
            new LocalGet(0),
            new LocalGet(1),
            new Float32Maximum(),
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
                Assert.AreEqual(Math.Max(comparand, value), exports.Test(comparand, value));

            foreach (var value in values)
                Assert.AreEqual(Math.Max(value, comparand), exports.Test(value, comparand));
        }
    }
}

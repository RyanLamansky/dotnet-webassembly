using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float64SquareRoot"/> instruction.
/// </summary>
[TestClass]
public class Float64SquareRootTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float64SquareRoot"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float64SquareRoot_Compiled()
    {
        var exports = CompilerTestBase<double>.CreateInstance(
            new LocalGet(0),
            new Float64SquareRoot(),
            new End());

        foreach (var value in new[] { 1f, -1f, -Math.PI, Math.PI })
            Assert.AreEqual(Math.Sqrt(value), exports.Test(value));
    }
}

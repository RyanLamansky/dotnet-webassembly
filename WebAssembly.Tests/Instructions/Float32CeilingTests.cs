using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float32Ceiling"/> instruction.
/// </summary>
[TestClass]
public class Float32CeilingTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float32Ceiling"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float32Ceiling_Compiled()
    {
        var exports = CompilerTestBase<float>.CreateInstance(
            new LocalGet(0),
            new Float32Ceiling(),
            new End());

        foreach (var value in Samples.Single)
            Assert.AreEqual((float)Math.Ceiling(value), exports.Test(value));
    }
}

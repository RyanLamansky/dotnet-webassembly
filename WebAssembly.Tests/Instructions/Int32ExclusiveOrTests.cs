﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int32ExclusiveOr"/> instruction.
/// </summary>
[TestClass]
public class Int32ExclusiveOrTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int32ExclusiveOr"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int32ExclusiveOr_Compiled()
    {
        const int or = 0xF;

        var exports = CompilerTestBase<int>.CreateInstance(
            new LocalGet(0),
            new Int32Constant(or),
            new Int32ExclusiveOr(),
            new End());

        foreach (var value in new[] { 0x00, 0x0F, 0xF0, 0xFF, })
            Assert.AreEqual(value ^ or, exports.Test(value));
    }
}

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int32Equal"/> instruction.
/// </summary>
[TestClass]
public class Int32EqualTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Int32Equal"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int32Equal_Compiled()
    {
        var exports = ComparisonTestBase<int>.CreateInstance(
            new LocalGet(0),
            new LocalGet(1),
            new Int32Equal(),
            new End());

        var values = new int[]
        {
                -1,
                0,
                1,
                0x00,
                0x0F,
                0xF0,
                0xFF,
                byte.MaxValue,
                short.MinValue,
                short.MaxValue,
                ushort.MaxValue,
                int.MinValue,
                int.MaxValue,
        };

        foreach (var comparand in values)
        {
            foreach (var value in values)
                Assert.AreEqual(comparand == value, exports.Test(comparand, value) != 0);

            foreach (var value in values)
                Assert.AreEqual(value == comparand, exports.Test(value, comparand) != 0);
        }
    }
}

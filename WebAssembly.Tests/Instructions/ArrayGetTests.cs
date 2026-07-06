using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayGet"/> instruction.
/// </summary>
[TestClass]
public class ArrayGetTests
{
    /// <summary>Tests <see cref="ArrayGet"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayGet_OpCodeAndEquality()
    {
        var instruction = new ArrayGet { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayGet, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayGet { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayGet { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayGet { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

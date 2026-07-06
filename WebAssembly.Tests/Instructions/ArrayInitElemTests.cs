using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayInitElem"/> instruction.
/// </summary>
[TestClass]
public class ArrayInitElemTests
{
    /// <summary>Tests <see cref="ArrayInitElem"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayInitElem_OpCodeAndEquality()
    {
        var instruction = new ArrayInitElem { Type = 3, Element = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayInitElem, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(1u, instruction.Element);
        Assert.AreEqual(new ArrayInitElem { Type = 3, Element = 1 }, instruction);
        Assert.AreNotEqual(new ArrayInitElem { Type = 3, Element = 2 }, instruction);
        Assert.AreNotEqual(new ArrayInitElem { Type = 4, Element = 1 }, instruction);
        Assert.AreEqual(new ArrayInitElem { Type = 3, Element = 1 }.GetHashCode(), instruction.GetHashCode());
    }
}

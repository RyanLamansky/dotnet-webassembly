using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayNewElem"/> instruction.
/// </summary>
[TestClass]
public class ArrayNewElemTests
{
    /// <summary>Tests <see cref="ArrayNewElem"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayNewElem_OpCodeAndEquality()
    {
        var instruction = new ArrayNewElem { Type = 3, Element = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayNewElem, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(1u, instruction.Element);
        Assert.AreEqual(new ArrayNewElem { Type = 3, Element = 1 }, instruction);
        Assert.AreNotEqual(new ArrayNewElem { Type = 3, Element = 2 }, instruction);
        Assert.AreNotEqual(new ArrayNewElem { Type = 4, Element = 1 }, instruction);
        Assert.AreEqual(new ArrayNewElem { Type = 3, Element = 1 }.GetHashCode(), instruction.GetHashCode());
    }
}

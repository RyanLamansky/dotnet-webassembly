using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayFill"/> instruction.
/// </summary>
[TestClass]
public class ArrayFillTests
{
    /// <summary>Tests <see cref="ArrayFill"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayFill_OpCodeAndEquality()
    {
        var instruction = new ArrayFill { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayFill, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayFill { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayFill { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayFill { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

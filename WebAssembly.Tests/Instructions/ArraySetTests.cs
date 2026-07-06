using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArraySet"/> instruction.
/// </summary>
[TestClass]
public class ArraySetTests
{
    /// <summary>Tests <see cref="ArraySet"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArraySet_OpCodeAndEquality()
    {
        var instruction = new ArraySet { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArraySet, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArraySet { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArraySet { Type = 4 }, instruction);
        Assert.AreEqual(new ArraySet { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

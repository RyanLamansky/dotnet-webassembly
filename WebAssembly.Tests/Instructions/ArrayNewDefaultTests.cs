using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayNewDefault"/> instruction.
/// </summary>
[TestClass]
public class ArrayNewDefaultTests
{
    /// <summary>Tests <see cref="ArrayNewDefault"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayNewDefault_OpCodeAndEquality()
    {
        var instruction = new ArrayNewDefault { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayNewDefault, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayNewDefault { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayNewDefault { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayNewDefault { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

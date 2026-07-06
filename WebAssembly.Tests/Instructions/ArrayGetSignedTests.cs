using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayGetSigned"/> instruction.
/// </summary>
[TestClass]
public class ArrayGetSignedTests
{
    /// <summary>Tests <see cref="ArrayGetSigned"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayGetSigned_OpCodeAndEquality()
    {
        var instruction = new ArrayGetSigned { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayGetSigned, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayGetSigned { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayGetSigned { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayGetSigned { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

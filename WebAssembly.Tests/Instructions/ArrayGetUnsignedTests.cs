using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayGetUnsigned"/> instruction.
/// </summary>
[TestClass]
public class ArrayGetUnsignedTests
{
    /// <summary>Tests <see cref="ArrayGetUnsigned"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayGetUnsigned_OpCodeAndEquality()
    {
        var instruction = new ArrayGetUnsigned { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayGetUnsigned, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayGetUnsigned { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayGetUnsigned { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayGetUnsigned { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

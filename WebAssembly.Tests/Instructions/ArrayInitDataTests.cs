using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayInitData"/> instruction.
/// </summary>
[TestClass]
public class ArrayInitDataTests
{
    /// <summary>Tests <see cref="ArrayInitData"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayInitData_OpCodeAndEquality()
    {
        var instruction = new ArrayInitData { Type = 3, Data = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayInitData, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(1u, instruction.Data);
        Assert.AreEqual(new ArrayInitData { Type = 3, Data = 1 }, instruction);
        Assert.AreNotEqual(new ArrayInitData { Type = 3, Data = 2 }, instruction);
        Assert.AreNotEqual(new ArrayInitData { Type = 4, Data = 1 }, instruction);
        Assert.AreEqual(new ArrayInitData { Type = 3, Data = 1 }.GetHashCode(), instruction.GetHashCode());
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayNewData"/> instruction.
/// </summary>
[TestClass]
public class ArrayNewDataTests
{
    /// <summary>Tests <see cref="ArrayNewData"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayNewData_OpCodeAndEquality()
    {
        var instruction = new ArrayNewData { Type = 3, Data = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayNewData, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(1u, instruction.Data);
        Assert.AreEqual(new ArrayNewData { Type = 3, Data = 1 }, instruction);
        Assert.AreNotEqual(new ArrayNewData { Type = 3, Data = 2 }, instruction);
        Assert.AreNotEqual(new ArrayNewData { Type = 4, Data = 1 }, instruction);
        Assert.AreEqual(new ArrayNewData { Type = 3, Data = 1 }.GetHashCode(), instruction.GetHashCode());
    }
}

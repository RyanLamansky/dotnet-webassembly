using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayCopy"/> instruction.
/// </summary>
[TestClass]
public class ArrayCopyTests
{
    /// <summary>Tests <see cref="ArrayCopy"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayCopy_OpCodeAndEquality()
    {
        var instruction = new ArrayCopy { DestinationType = 0, SourceType = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayCopy, instruction.GCOpCode);
        Assert.AreEqual(0u, instruction.DestinationType);
        Assert.AreEqual(1u, instruction.SourceType);
        Assert.AreEqual(new ArrayCopy { DestinationType = 0, SourceType = 1 }, instruction);
        Assert.AreNotEqual(new ArrayCopy { DestinationType = 0, SourceType = 2 }, instruction);
        Assert.AreNotEqual(new ArrayCopy { DestinationType = 1, SourceType = 1 }, instruction);
        Assert.AreEqual(new ArrayCopy { DestinationType = 0, SourceType = 1 }.GetHashCode(), instruction.GetHashCode());
    }
}

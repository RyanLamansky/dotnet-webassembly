using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayLen"/> instruction.
/// </summary>
[TestClass]
public class ArrayLenTests
{
    /// <summary>Tests <see cref="ArrayLen"/> opcode and equality.</summary>
    [TestMethod]
    public void ArrayLen_OpCodeAndEquality()
    {
        var instruction = new ArrayLen();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayLen, instruction.GCOpCode);
        Assert.AreEqual(new ArrayLen(), instruction);
        Assert.AreEqual(new ArrayLen().GetHashCode(), instruction.GetHashCode());
    }
}

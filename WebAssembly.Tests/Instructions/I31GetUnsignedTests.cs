using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="I31GetUnsigned"/> instruction.
/// </summary>
[TestClass]
public class I31GetUnsignedTests
{
    /// <summary>Tests <see cref="I31GetUnsigned"/> opcode and equality.</summary>
    [TestMethod]
    public void I31GetUnsigned_OpCodeAndEquality()
    {
        var instruction = new I31GetUnsigned();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.I31GetUnsigned, instruction.GCOpCode);
        Assert.AreEqual(new I31GetUnsigned(), instruction);
        Assert.AreEqual(new I31GetUnsigned().GetHashCode(), instruction.GetHashCode());
    }
}

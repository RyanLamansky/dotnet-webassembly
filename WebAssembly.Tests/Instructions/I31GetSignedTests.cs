using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="I31GetSigned"/> instruction.
/// </summary>
[TestClass]
public class I31GetSignedTests
{
    /// <summary>Tests <see cref="I31GetSigned"/> opcode and equality.</summary>
    [TestMethod]
    public void I31GetSigned_OpCodeAndEquality()
    {
        var instruction = new I31GetSigned();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.I31GetSigned, instruction.GCOpCode);
        Assert.AreEqual(new I31GetSigned(), instruction);
        Assert.AreEqual(new I31GetSigned().GetHashCode(), instruction.GetHashCode());
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefI31"/> instruction.
/// </summary>
[TestClass]
public class RefI31Tests
{
    /// <summary>Tests <see cref="RefI31"/> opcode and equality.</summary>
    [TestMethod]
    public void RefI31_OpCodeAndEquality()
    {
        var instruction = new RefI31();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.RefI31, instruction.GCOpCode);
        Assert.AreEqual(new RefI31(), instruction);
        Assert.AreEqual(new RefI31().GetHashCode(), instruction.GetHashCode());
    }
}

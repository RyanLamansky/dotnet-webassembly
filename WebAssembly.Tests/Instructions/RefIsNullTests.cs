using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefIsNull"/> instruction.
/// </summary>
[TestClass]
public class RefIsNullTests
{
    /// <summary>Tests the <see cref="RefIsNull"/> opcode and equality.</summary>
    [TestMethod]
    public void RefIsNull_OpCodeAndEquality()
    {
        var instruction = new RefIsNull();
        Assert.AreEqual(OpCode.RefIsNull, instruction.OpCode);
        Assert.AreEqual(new RefIsNull(), instruction);
    }
}

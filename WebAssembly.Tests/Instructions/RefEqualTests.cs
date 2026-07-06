using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefEqual"/> instruction.
/// </summary>
[TestClass]
public class RefEqualTests
{
    /// <summary>Tests <see cref="RefEqual"/> opcode.</summary>
    [TestMethod]
    public void RefEqual_OpCode()
    {
        var instruction = new RefEqual();
        Assert.AreEqual(OpCode.RefEqual, instruction.OpCode);
        Assert.AreEqual(new RefEqual(), instruction);
    }
}

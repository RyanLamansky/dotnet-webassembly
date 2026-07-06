using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="AnyConvertExtern"/> instruction.
/// </summary>
[TestClass]
public class AnyConvertExternTests
{
    /// <summary>Tests <see cref="AnyConvertExtern"/> opcode and equality.</summary>
    [TestMethod]
    public void AnyConvertExtern_OpCodeAndEquality()
    {
        var instruction = new AnyConvertExtern();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.AnyConvertExtern, instruction.GCOpCode);
        Assert.AreEqual(new AnyConvertExtern(), instruction);
        Assert.AreEqual(new AnyConvertExtern().GetHashCode(), instruction.GetHashCode());
    }
}

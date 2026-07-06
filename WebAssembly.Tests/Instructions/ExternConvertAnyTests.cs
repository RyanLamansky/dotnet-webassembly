using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ExternConvertAny"/> instruction.
/// </summary>
[TestClass]
public class ExternConvertAnyTests
{
    /// <summary>Tests <see cref="ExternConvertAny"/> opcode and equality.</summary>
    [TestMethod]
    public void ExternConvertAny_OpCodeAndEquality()
    {
        var instruction = new ExternConvertAny();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ExternConvertAny, instruction.GCOpCode);
        Assert.AreEqual(new ExternConvertAny(), instruction);
        Assert.AreEqual(new ExternConvertAny().GetHashCode(), instruction.GetHashCode());
    }
}

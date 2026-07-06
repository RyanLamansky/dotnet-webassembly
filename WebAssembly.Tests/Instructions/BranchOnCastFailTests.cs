using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="BranchOnCastFail"/> instruction.
/// </summary>
[TestClass]
public class BranchOnCastFailTests
{
    /// <summary>Tests <see cref="BranchOnCastFail"/> opcode, cast-flags decoding, immediates, and equality.</summary>
    [TestMethod]
    public void BranchOnCastFail_OpCodeAndEquality()
    {
        var instruction = new BranchOnCastFail
        {
            CastFlags = 0b01,
            Index = 0,
            FromHeapType = WebAssemblyHeapType.Any,
            ToTypeIndex = 2,
        };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.BranchOnCastFail, instruction.GCOpCode);
        Assert.IsTrue(instruction.FromNullable);
        Assert.IsFalse(instruction.ToNullable);
        Assert.AreEqual(2u, instruction.ToTypeIndex);
        Assert.IsNull(instruction.ToHeapType);

        var same = new BranchOnCastFail
        {
            CastFlags = 0b01,
            Index = 0,
            FromHeapType = WebAssemblyHeapType.Any,
            ToTypeIndex = 2,
        };
        Assert.AreEqual(same, instruction);
        Assert.AreEqual(same.GetHashCode(), instruction.GetHashCode());
        Assert.AreNotEqual(new BranchOnCastFail { CastFlags = 0b01, Index = 0, FromHeapType = WebAssemblyHeapType.Any, ToTypeIndex = 3 }, instruction);
    }
}

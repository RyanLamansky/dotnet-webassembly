using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="BranchOnCast"/> instruction.
/// </summary>
[TestClass]
public class BranchOnCastTests
{
    /// <summary>Tests <see cref="BranchOnCast"/> opcode, cast-flags decoding, immediates, and equality.</summary>
    [TestMethod]
    public void BranchOnCast_OpCodeAndEquality()
    {
        var instruction = new BranchOnCast
        {
            CastFlags = 0b11,
            Index = 1,
            FromHeapType = WebAssemblyHeapType.Any,
            ToHeapType = WebAssemblyHeapType.Struct,
        };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.BranchOnCast, instruction.GCOpCode);
        Assert.IsTrue(instruction.FromNullable);
        Assert.IsTrue(instruction.ToNullable);
        Assert.AreEqual(1u, instruction.Index);
        Assert.AreEqual(WebAssemblyHeapType.Any, instruction.FromHeapType);
        Assert.AreEqual(WebAssemblyHeapType.Struct, instruction.ToHeapType);

        var same = new BranchOnCast
        {
            CastFlags = 0b11,
            Index = 1,
            FromHeapType = WebAssemblyHeapType.Any,
            ToHeapType = WebAssemblyHeapType.Struct,
        };
        Assert.AreEqual(same, instruction);
        Assert.AreEqual(same.GetHashCode(), instruction.GetHashCode());
        Assert.AreNotEqual(new BranchOnCast { CastFlags = 0b01, Index = 1, FromHeapType = WebAssemblyHeapType.Any, ToHeapType = WebAssemblyHeapType.Struct }, instruction);
        Assert.AreNotEqual<BranchCastInstruction>(new BranchOnCastFail
        {
            CastFlags = 0b11,
            Index = 1,
            FromHeapType = WebAssemblyHeapType.Any,
            ToHeapType = WebAssemblyHeapType.Struct,
        }, instruction);
    }
}

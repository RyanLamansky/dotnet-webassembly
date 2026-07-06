using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TryTable"/> instruction.
/// </summary>
[TestClass]
public class TryTableTests
{
    /// <summary>Tests <see cref="TryTable"/> catch-clause round-tripping and equality.</summary>
    [TestMethod]
    public void TryTable_Equality()
    {
        var instruction = new TryTable(BlockType.Int32);
        instruction.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 2));
        instruction.Catches.Add(new CatchClause(CatchKind.CatchAll, 1));
        Assert.AreEqual(OpCode.TryTable, instruction.OpCode);

        var same = new TryTable(BlockType.Int32);
        same.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 2));
        same.Catches.Add(new CatchClause(CatchKind.CatchAll, 1));
        Assert.AreEqual(same, instruction);
        Assert.AreEqual(same.GetHashCode(), instruction.GetHashCode());

        var different = new TryTable(BlockType.Int32);
        different.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 3));
        Assert.AreNotEqual(different, instruction);
    }

    /// <summary>Tests that <see cref="CatchClause"/> kinds carry the right fields.</summary>
    [TestMethod]
    public void TryTable_CatchClauseFields()
    {
        var catchRef = new CatchClause(CatchKind.CatchRef, 4, tag: 5);
        Assert.AreEqual(CatchKind.CatchRef, catchRef.Kind);
        Assert.AreEqual(5u, catchRef.Tag);
        Assert.AreEqual(4u, catchRef.Label);

        var catchAllRef = new CatchClause(CatchKind.CatchAllRef, 7);
        Assert.AreEqual(CatchKind.CatchAllRef, catchAllRef.Kind);
        Assert.AreEqual(7u, catchAllRef.Label);
        Assert.AreNotEqual(catchRef, catchAllRef);
        Assert.AreEqual(new CatchClause(CatchKind.CatchAllRef, 7), catchAllRef);
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableInit"/> instruction.
/// </summary>
[TestClass]
public class TableInitTests
{
    /// <summary>Tests <see cref="TableInit"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableInit_Equality()
    {
        var instruction = new TableInit { SegmentIndex = 2, TableIndex = 3 };
        Assert.AreEqual(2u, instruction.SegmentIndex);
        Assert.AreEqual(3u, instruction.TableIndex);
        Assert.AreEqual(new TableInit { SegmentIndex = 2, TableIndex = 3 }, instruction);
        Assert.AreNotEqual(new TableInit { SegmentIndex = 2, TableIndex = 4 }, instruction);
        Assert.AreNotEqual(new TableInit { SegmentIndex = 9, TableIndex = 3 }, instruction);
    }
}

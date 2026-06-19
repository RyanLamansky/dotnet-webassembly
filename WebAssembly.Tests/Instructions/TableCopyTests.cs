using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableCopy"/> instruction.
/// </summary>
[TestClass]
public class TableCopyTests
{
    /// <summary>Tests <see cref="TableCopy"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableCopy_Equality()
    {
        var instruction = new TableCopy { DestinationTableIndex = 1, SourceTableIndex = 2 };
        Assert.AreEqual(1u, instruction.DestinationTableIndex);
        Assert.AreEqual(2u, instruction.SourceTableIndex);
        Assert.AreEqual(new TableCopy { DestinationTableIndex = 1, SourceTableIndex = 2 }, instruction);
        Assert.AreNotEqual(new TableCopy { DestinationTableIndex = 2, SourceTableIndex = 1 }, instruction);
    }
}

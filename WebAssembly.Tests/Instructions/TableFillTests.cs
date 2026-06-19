using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableFill"/> instruction.
/// </summary>
[TestClass]
public class TableFillTests
{
    /// <summary>Tests <see cref="TableFill"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableFill_Equality()
    {
        var instruction = new TableFill { TableIndex = 3 };
        Assert.AreEqual(3u, instruction.TableIndex);
        Assert.AreEqual(new TableFill { TableIndex = 3 }, instruction);
        Assert.AreNotEqual(new TableFill { TableIndex = 4 }, instruction);
        Assert.AreEqual(new TableFill { TableIndex = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

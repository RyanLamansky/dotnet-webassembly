using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableGrow"/> instruction.
/// </summary>
[TestClass]
public class TableGrowTests
{
    /// <summary>Tests <see cref="TableGrow"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableGrow_Equality()
    {
        var instruction = new TableGrow { TableIndex = 3 };
        Assert.AreEqual(3u, instruction.TableIndex);
        Assert.AreEqual(new TableGrow { TableIndex = 3 }, instruction);
        Assert.AreNotEqual(new TableGrow { TableIndex = 4 }, instruction);
        Assert.AreEqual(new TableGrow { TableIndex = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableSet"/> instruction.
/// </summary>
[TestClass]
public class TableSetTests
{
    /// <summary>Tests <see cref="TableSet"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableSet_Equality()
    {
        var instruction = new TableSet { TableIndex = 3 };
        Assert.AreEqual(3u, instruction.TableIndex);
        Assert.AreEqual(new TableSet { TableIndex = 3 }, instruction);
        Assert.AreNotEqual(new TableSet { TableIndex = 4 }, instruction);
        Assert.AreEqual(new TableSet { TableIndex = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

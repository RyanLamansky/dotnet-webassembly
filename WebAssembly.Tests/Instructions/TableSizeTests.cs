using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableSize"/> instruction.
/// </summary>
[TestClass]
public class TableSizeTests
{
    /// <summary>Tests <see cref="TableSize"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableSize_Equality()
    {
        var instruction = new TableSize { TableIndex = 3 };
        Assert.AreEqual(3u, instruction.TableIndex);
        Assert.AreEqual(new TableSize { TableIndex = 3 }, instruction);
        Assert.AreNotEqual(new TableSize { TableIndex = 4 }, instruction);
        Assert.AreEqual(new TableSize { TableIndex = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

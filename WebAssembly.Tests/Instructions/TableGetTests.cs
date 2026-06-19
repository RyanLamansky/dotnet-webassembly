using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableGet"/> instruction.
/// </summary>
[TestClass]
public class TableGetTests
{
    /// <summary>Tests <see cref="TableGet"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void TableGet_Equality()
    {
        var instruction = new TableGet { TableIndex = 3 };
        Assert.AreEqual(3u, instruction.TableIndex);
        Assert.AreEqual(new TableGet { TableIndex = 3 }, instruction);
        Assert.AreNotEqual(new TableGet { TableIndex = 4 }, instruction);
        Assert.AreEqual(new TableGet { TableIndex = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

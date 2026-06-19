using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefFunc"/> instruction.
/// </summary>
[TestClass]
public class RefFuncTests
{
    /// <summary>Tests <see cref="RefFunc"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void RefFunc_Equality()
    {
        var instruction = new RefFunc { Index = 3 };
        Assert.AreEqual(3u, instruction.Index);
        Assert.AreEqual(new RefFunc { Index = 3 }, instruction);
        Assert.AreNotEqual(new RefFunc { Index = 4 }, instruction);
        Assert.AreEqual(new RefFunc { Index = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

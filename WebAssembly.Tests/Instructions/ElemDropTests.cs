using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ElemDrop"/> instruction.
/// </summary>
[TestClass]
public class ElemDropTests
{
    /// <summary>Tests <see cref="ElemDrop"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void ElemDrop_Equality()
    {
        var instruction = new ElemDrop { SegmentIndex = 3 };
        Assert.AreEqual(3u, instruction.SegmentIndex);
        Assert.AreEqual(new ElemDrop { SegmentIndex = 3 }, instruction);
        Assert.AreNotEqual(new ElemDrop { SegmentIndex = 4 }, instruction);
        Assert.AreEqual(new ElemDrop { SegmentIndex = 3 }.GetHashCode(), instruction.GetHashCode());
    }
}

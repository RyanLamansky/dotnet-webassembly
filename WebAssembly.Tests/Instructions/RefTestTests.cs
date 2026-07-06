using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefTest"/> instruction.
/// </summary>
[TestClass]
public class RefTestTests
{
    /// <summary>Tests <see cref="RefTest"/> opcode selection, immediates, and equality.</summary>
    [TestMethod]
    public void RefTest_OpCodeAndEquality()
    {
        var nonNull = new RefTest { HeapType = WebAssemblyHeapType.Struct };
        Assert.AreEqual(OpCode.GCPrefix, nonNull.OpCode);
        Assert.AreEqual(GCOpCode.RefTest, nonNull.GCOpCode);
        Assert.IsFalse(nonNull.Nullable);
        Assert.AreEqual(WebAssemblyHeapType.Struct, nonNull.HeapType);
        Assert.IsNull(nonNull.TypeIndex);

        var nullable = new RefTest(true) { TypeIndex = 3 };
        Assert.AreEqual(GCOpCode.RefTestNull, nullable.GCOpCode);
        Assert.IsTrue(nullable.Nullable);
        Assert.AreEqual(3u, nullable.TypeIndex);
        Assert.IsNull(nullable.HeapType);

        Assert.AreEqual(new RefTest { HeapType = WebAssemblyHeapType.Struct }, nonNull);
        Assert.AreNotEqual(new RefTest(true) { HeapType = WebAssemblyHeapType.Struct }, nonNull);
        Assert.AreEqual(new RefTest { HeapType = WebAssemblyHeapType.Struct }.GetHashCode(), nonNull.GetHashCode());
    }
}

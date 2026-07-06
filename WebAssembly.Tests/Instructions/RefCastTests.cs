using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefCast"/> instruction.
/// </summary>
[TestClass]
public class RefCastTests
{
    /// <summary>Tests <see cref="RefCast"/> opcode selection, immediates, and equality.</summary>
    [TestMethod]
    public void RefCast_OpCodeAndEquality()
    {
        var nonNull = new RefCast { HeapType = WebAssemblyHeapType.Array };
        Assert.AreEqual(OpCode.GCPrefix, nonNull.OpCode);
        Assert.AreEqual(GCOpCode.RefCast, nonNull.GCOpCode);
        Assert.IsFalse(nonNull.Nullable);

        var nullable = new RefCast(true) { HeapType = WebAssemblyHeapType.Any };
        Assert.AreEqual(GCOpCode.RefCastNull, nullable.GCOpCode);
        Assert.IsTrue(nullable.Nullable);

        Assert.AreEqual(new RefCast { HeapType = WebAssemblyHeapType.Array }, nonNull);
        Assert.AreNotEqual(new RefCast { HeapType = WebAssemblyHeapType.Struct }, nonNull);
        Assert.AreEqual(new RefCast { HeapType = WebAssemblyHeapType.Array }.GetHashCode(), nonNull.GetHashCode());
    }
}

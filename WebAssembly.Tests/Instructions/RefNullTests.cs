using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefNull"/> instruction.
/// </summary>
[TestClass]
public class RefNullTests
{
    /// <summary>Tests <see cref="RefNull"/> reference-type round-tripping and equality.</summary>
    [TestMethod]
    public void RefNull_Equality()
    {
        var instruction = new RefNull(WebAssemblyValueType.ExternRef);
        Assert.AreEqual(WebAssemblyValueType.ExternRef, instruction.Type);
        Assert.AreEqual(new RefNull(WebAssemblyValueType.ExternRef), instruction);
        Assert.AreNotEqual(new RefNull(WebAssemblyValueType.FuncRef), instruction);
        Assert.AreEqual(WebAssemblyValueType.FuncRef, new RefNull().Type);
    }
}

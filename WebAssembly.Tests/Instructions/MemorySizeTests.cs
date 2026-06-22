using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="MemorySize"/> instruction.
/// </summary>
[TestClass]
public class MemorySizeTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="MemorySize"/> instruction.
    /// </summary>
    [TestMethod]
    public void CurrentMemory_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Returns =
            [
                    WebAssemblyValueType.Int32,
                ],
        });
        module.Functions.Add(new Function
        {
        });
        module.Exports.Add(new Export
        {
            Name = "Test",
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                    new MemorySize(),
                    new End(),
            ]
        });
        module.Memories.Add(new Memory(1, 1));

        var compiled = module.ToInstance<dynamic>();

        var exports = compiled.Exports;

        Assert.AreEqual<int>(1, exports.Test());
    }

    /// <summary>
    /// A non-zero memory index is permitted in the object model but must be rejected by the compiler.
    /// </summary>
    [TestMethod]
    public void CurrentMemory_NonZeroMemoryIndex_RejectedByCompiler()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = "Test" });
        module.Codes.Add(new FunctionBody
        {
            Code = [new MemorySize { MemoryIndex = 1 }, new End()],
        });
        module.Memories.Add(new Memory(1, 1));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<dynamic>());
    }

    /// <summary>The obsolete <c>Reserved</c> alias forwards to <see cref="MemorySize.MemoryIndex"/>.</summary>
    [TestMethod]
    public void CurrentMemory_ReservedAlias_ForwardsToMemoryIndex()
    {
        var instruction = new MemorySize { MemoryIndex = 3 };
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.AreEqual((byte)3, instruction.Reserved);
        instruction.Reserved = 7;
#pragma warning restore CS0618
        Assert.AreEqual(7u, instruction.MemoryIndex);
    }
}

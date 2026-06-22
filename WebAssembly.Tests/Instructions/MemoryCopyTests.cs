using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="MemoryCopy"/> instruction.
/// </summary>
[TestClass]
public class MemoryCopyTests
{
    /// <summary>Export that copies memory and reads a byte.</summary>
    public abstract class MemoryCopyExport
    {
        /// <summary>Copies len bytes from src to dst in memory, then returns the byte at readOffset.</summary>
        public abstract int Test(int dst, int src, int len, int readOffset);
    }

    /// <summary>
    /// Tests that <see cref="MemoryCopy"/> moves bytes correctly.
    /// </summary>
    [TestMethod]
    public void MemoryCopy_CopiesBytesCorrectly()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(MemoryCopyExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // Store value 42 at address 0.
                new Int32Constant(0),
                new Int32Constant(42),
                new Int32Store8 { Offset = 0 },
                // memory.copy(dst=param0, src=param1, len=param2)
                new LocalGet(0),
                new LocalGet(1),
                new LocalGet(2),
                new MemoryCopy(),
                // load byte at param3
                new LocalGet(3),
                new Int32Load8Unsigned { Offset = 0 },
                new End(),
            ],
        });

        var compiled = module.ToInstance<MemoryCopyExport>();
        // Copy 1 byte from address 0 (value 42) to address 100; read back from 100.
        Assert.AreEqual(42, compiled.Exports.Test(100, 0, 1, 100));
    }

    /// <summary>
    /// A non-zero source or destination memory index (multi-memory is not supported) is permitted in the object
    /// model but must be rejected by the compiler.
    /// </summary>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void MemoryCopy_NonZeroMemoryIndex_RejectedByCompiler(bool badDestination, bool badSource)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(MemoryCopyExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new LocalGet(1),
                new LocalGet(2),
                new MemoryCopy
                {
                    DestinationMemoryIndex = badDestination ? 1u : 0u,
                    SourceMemoryIndex = badSource ? 1u : 0u,
                },
                new LocalGet(3),
                new Int32Load8Unsigned { Offset = 0 },
                new End(),
            ],
        });

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<MemoryCopyExport>());
    }
}

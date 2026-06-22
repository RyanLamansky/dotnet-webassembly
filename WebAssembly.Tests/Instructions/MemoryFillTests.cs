using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="MemoryFill"/> instruction.
/// </summary>
[TestClass]
public class MemoryFillTests
{
    /// <summary>Export that fills memory and reads back a byte.</summary>
    public abstract class MemoryFillExport
    {
        /// <summary>Fills len bytes at dst with val, then returns the byte at readOffset.</summary>
        public abstract int Test(int dst, int val, int len, int readOffset);
    }

    /// <summary>
    /// Tests that <see cref="MemoryFill"/> writes the correct byte value over the requested range only.
    /// </summary>
    [TestMethod]
    public void MemoryFill_FillsBytesCorrectly()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(MemoryFillExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // memory.fill(dst=param0, val=param1, len=param2)
                new LocalGet(0),
                new LocalGet(1),
                new LocalGet(2),
                new MemoryFill(),
                // load byte at param3
                new LocalGet(3),
                new Int32Load8Unsigned { Offset = 0 },
                new End(),
            ],
        });

        var compiled = module.ToInstance<MemoryFillExport>();
        // Fill 4 bytes at address 10 with value 0xAB; read first filled byte.
        Assert.AreEqual(0xAB, compiled.Exports.Test(10, 0xAB, 4, 10));
        // Read the last filled byte (address 13).
        Assert.AreEqual(0xAB, compiled.Exports.Test(10, 0xAB, 4, 13));
        // Address 14 should still be 0 (not filled).
        Assert.AreEqual(0, compiled.Exports.Test(10, 0xAB, 4, 14));
    }

    /// <summary>
    /// A non-zero memory index (multi-memory is not supported) is permitted in the object model but must be
    /// rejected by the compiler, whose output is runnable code.
    /// </summary>
    [TestMethod]
    public void MemoryFill_NonZeroMemoryIndex_RejectedByCompiler()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(MemoryFillExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new LocalGet(1),
                new LocalGet(2),
                new MemoryFill { MemoryIndex = 1 },
                new LocalGet(3),
                new Int32Load8Unsigned { Offset = 0 },
                new End(),
            ],
        });

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<MemoryFillExport>());
    }

    /// <summary>
    /// The memory index is a <c>varuint32</c> in the spec; the object model must round-trip a value that the
    /// former <see cref="byte"/> field would have truncated, leaving validity to the compiler.
    /// </summary>
    [TestMethod]
    public void MemoryFill_MemoryIndex_RoundTripsFaithfully()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [] });
        module.Functions.Add(new Function());
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0),
                new Int32Constant(0),
                new MemoryFill { MemoryIndex = 300 },
                new End(),
            ],
        });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        stream.Position = 0;
        var roundTripped = Module.ReadFromBinary(stream);

        var fill = (MemoryFill)roundTripped.Codes[0].Code[3];
        Assert.AreEqual(300u, fill.MemoryIndex);
    }
}

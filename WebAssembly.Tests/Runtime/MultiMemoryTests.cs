using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the WebAssembly 3.0 multiple-memories feature: memory-index immediates, their binary encoding, and
/// runtime selection of the correct memory by loads, stores, and the bulk <c>memory.*</c> operations.
/// </summary>
[TestClass]
public class MultiMemoryTests
{
    /// <summary>Exposes two independent linear memories through a copy/load/size surface.</summary>
    public abstract class TwoMemoryExport
    {
        /// <summary>Stores <paramref name="value"/> into memory 1, copies it into memory 0, and returns memory 0's byte word.</summary>
        public abstract int CopyFromMemory1ToMemory0(int value);

        /// <summary>Reads the i32 at address 0 of memory 0.</summary>
        public abstract int LoadMemory0();

        /// <summary>Reads the i32 at address 0 of memory 1.</summary>
        public abstract int LoadMemory1();

        /// <summary>Returns the size (in pages) of memory 0.</summary>
        public abstract int SizeMemory0();

        /// <summary>Returns the size (in pages) of memory 1.</summary>
        public abstract int SizeMemory1();

        /// <summary>Grows memory 1 by <paramref name="pages"/> and returns the previous size.</summary>
        public abstract int GrowMemory1(int pages);
    }

    private static Instance<TwoMemoryExport> CreateTwoMemoryInstance()
    {
        var module = new Module();
        // Memory 0 has a single page; memory 1 can grow to two so the grow test observes a change.
        module.Memories.Add(new Memory(1, 1));
        module.Memories.Add(new Memory(1, 2));

        // Type 0: (i32) -> i32; type 1: () -> i32; type 2: () -> ().
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });

        void AddFunction(string name, uint typeIndex, params Instruction[] code)
        {
            module.Functions.Add(new Function { Type = typeIndex });
            module.Exports.Add(new Export { Name = name, Index = (uint)(module.Functions.Count - 1) });
            module.Codes.Add(new FunctionBody { Code = code });
        }

        AddFunction(nameof(TwoMemoryExport.CopyFromMemory1ToMemory0), 0,
            // memory1[0] = value
            new Int32Constant(0),
            new LocalGet(0),
            new Int32Store { MemoryIndex = 1 },
            // memory.copy dst=memory0, src=memory1, dst_addr=0, src_addr=0, len=4
            new Int32Constant(0),
            new Int32Constant(0),
            new Int32Constant(4),
            new MemoryCopy { DestinationMemoryIndex = 0, SourceMemoryIndex = 1 },
            // return memory0[0]
            new Int32Constant(0),
            new Int32Load { MemoryIndex = 0 },
            new End());

        AddFunction(nameof(TwoMemoryExport.LoadMemory0), 1,
            new Int32Constant(0), new Int32Load { MemoryIndex = 0 }, new End());

        AddFunction(nameof(TwoMemoryExport.LoadMemory1), 1,
            new Int32Constant(0), new Int32Load { MemoryIndex = 1 }, new End());

        AddFunction(nameof(TwoMemoryExport.SizeMemory0), 1,
            new MemorySize { MemoryIndex = 0 }, new End());

        AddFunction(nameof(TwoMemoryExport.SizeMemory1), 1,
            new MemorySize { MemoryIndex = 1 }, new End());

        AddFunction(nameof(TwoMemoryExport.GrowMemory1), 0,
            new LocalGet(0), new MemoryGrow { MemoryIndex = 1 }, new End());

        return module.ToInstance<TwoMemoryExport>();
    }

    /// <summary>
    /// A store to memory 1 and a copy into memory 0 land in the intended memories: memory 0 receives the copied
    /// word while a direct load of memory 1 sees the original store.
    /// </summary>
    [TestMethod]
    public void MultiMemory_LoadStoreCopy_SelectCorrectMemory()
    {
        using var instance = CreateTwoMemoryInstance();
        var exports = instance.Exports;

        const int value = 0x1234_5678;

        // Before the copy, memory 0 is untouched.
        Assert.AreEqual(0, exports.LoadMemory0());

        Assert.AreEqual(value, exports.CopyFromMemory1ToMemory0(value));

        // The store targeted memory 1; the copy propagated it to memory 0. Both now hold the value independently.
        Assert.AreEqual(value, exports.LoadMemory0());
        Assert.AreEqual(value, exports.LoadMemory1());
    }

    /// <summary>Each memory reports and grows its own size independently.</summary>
    [TestMethod]
    public void MultiMemory_SizeAndGrow_AreIndependent()
    {
        using var instance = CreateTwoMemoryInstance();
        var exports = instance.Exports;

        Assert.AreEqual(1, exports.SizeMemory0());
        Assert.AreEqual(1, exports.SizeMemory1());

        // Growing memory 1 leaves memory 0 unchanged.
        Assert.AreEqual(1, exports.GrowMemory1(1));
        Assert.AreEqual(1, exports.SizeMemory0());
        Assert.AreEqual(2, exports.SizeMemory1());

        // Memory 1's maximum is two pages, so a further grow fails and returns -1.
        Assert.AreEqual(-1, exports.GrowMemory1(1));
        Assert.AreEqual(2, exports.SizeMemory1());
    }

    /// <summary>
    /// A memarg targeting memory 0 (the default) serializes with the pre-3.0 minimal encoding: a plain alignment
    /// byte and offset, with no memory-index flag bit, so existing modules round-trip byte-identically.
    /// </summary>
    [TestMethod]
    public void Memarg_MemoryIndex0_UsesMinimalEncoding()
    {
        var withIndex = SerializeSingleLoad(new Int32Load { Flags = MemoryImmediateInstruction.Options.Align4, Offset = 8, MemoryIndex = 0 });
        var withoutIndex = SerializeSingleLoad(new Int32Load { Flags = MemoryImmediateInstruction.Options.Align4, Offset = 8 });

        // Setting MemoryIndex explicitly to 0 must produce the identical bytes as leaving it at its default.
        CollectionAssert.AreEqual(withoutIndex, withIndex);

        // The memarg itself is opcode 0x28 (i32.load), flags 0x02 (align4, no 0x40 flag bit), offset 0x08.
        AssertContainsSubsequence(withIndex, [0x28, 0x02, 0x08]);
    }

    /// <summary>
    /// A memarg targeting a non-default memory sets bit 6 of the alignment field and emits the memory index as a
    /// following <c>varuint32</c> (WebAssembly 3.0 encoding), and round-trips faithfully.
    /// </summary>
    [TestMethod]
    public void Memarg_NonZeroMemoryIndex_SetsFlagBitAndRoundTrips()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Load { Flags = MemoryImmediateInstruction.Options.Align4, Offset = 8, MemoryIndex = 1 },
                new End(),
            ],
        });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        var bytes = stream.ToArray();

        // Opcode 0x28, flags 0x42 (align4 | 0x40 flag bit), memory index 0x01, offset 0x08.
        AssertContainsSubsequence(bytes, [0x28, 0x42, 0x01, 0x08]);

        stream.Position = 0;
        var roundTripped = Module.ReadFromBinary(stream);
        var load = (Int32Load)roundTripped.Codes[0].Code[1];
        Assert.AreEqual(MemoryImmediateInstruction.Options.Align4, load.Flags);
        Assert.AreEqual(8u, load.Offset);
        Assert.AreEqual(1u, load.MemoryIndex);
    }

    /// <summary>
    /// A v128 memarg round-trips its memory index through the same bit-6 flag encoding as scalar loads, including
    /// the minimal (no flag bit) form when the index is 0.
    /// </summary>
    [TestMethod]
    [DataRow(0u)]
    [DataRow(1u)]
    public void SimdMemarg_MemoryIndex_RoundTrips(uint memoryIndex)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.V128] });
        module.Functions.Add(new Function());
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Load { Flags = 4, Offset = 8, MemoryIndex = memoryIndex },
                new End(),
            ],
        });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        var bytes = stream.ToArray();

        // Opcode prefix 0xFD, sub-opcode 0x00 (v128.load). Index 0 encodes flags 0x04; index 1 sets bit 6 (0x44)
        // and appends the index.
        AssertContainsSubsequence(bytes, memoryIndex == 0 ? [0xFD, 0x00, 0x04, 0x08] : [0xFD, 0x00, 0x44, 0x01, 0x08]);

        stream.Position = 0;
        var load = (V128Load)Module.ReadFromBinary(stream).Codes[0].Code[1];
        Assert.AreEqual(4u, load.Flags);
        Assert.AreEqual(8u, load.Offset);
        Assert.AreEqual(memoryIndex, load.MemoryIndex);
    }

    private static byte[] SerializeSingleLoad(Int32Load load)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Codes.Add(new FunctionBody
        {
            Code = [new Int32Constant(0), load, new End()],
        });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        return stream.ToArray();
    }

    private static void AssertContainsSubsequence(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i + needle.Length <= haystack.Length; i++)
        {
            if (haystack.Skip(i).Take(needle.Length).SequenceEqual(needle))
                return;
        }

        Assert.Fail($"Expected byte sequence {string.Join(" ", needle.Select(b => b.ToString("X2")))} was not found.");
    }
}

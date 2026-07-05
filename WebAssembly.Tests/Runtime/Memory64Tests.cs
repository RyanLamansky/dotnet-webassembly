using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the WebAssembly 3.0 memory64 feature: i64 addressing, u64 memarg offsets, honest 64-bit bounds checking
/// (trapping on effective-address overflow rather than wrapping), i64 <c>memory.size</c>/<c>memory.grow</c>, and
/// the reader/writer of 64-bit limits.
/// </summary>
[TestClass]
public class Memory64Tests
{
    /// <summary>Exposes a 64-bit linear memory through an i64-addressed load/store/size/grow surface.</summary>
    public abstract class Memory64Export
    {
        /// <summary>Reads the i32 at the given i64 address.</summary>
        public abstract int Load(long address);

        /// <summary>Reads the i32 at the given i64 address plus a static offset of 16.</summary>
        public abstract int LoadOffset16(long address);

        /// <summary>Writes <paramref name="value"/> to the i32 at the given i64 address.</summary>
        public abstract void Store(long address, int value);

        /// <summary>Returns the current size (in pages) as an i64.</summary>
        public abstract long Size();

        /// <summary>Grows the memory by <paramref name="pages"/> and returns the previous size (or -1) as an i64.</summary>
        public abstract long Grow(long pages);
    }

    private static Instance<Memory64Export> CreateInstance()
    {
        var module = new Module();
        // A 64-bit memory of one page, growable to two.
        module.Memories.Add(new Memory(1, 2) { Is64Bit = true });

        // Type 0: (i64)->i32; type 1: (i64,i32)->(); type 2: ()->i64; type 3: (i64)->i64.
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64], Returns = [WebAssemblyValueType.Int32] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64, WebAssemblyValueType.Int32] });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int64] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64], Returns = [WebAssemblyValueType.Int64] });

        void AddFunction(string name, uint typeIndex, params Instruction[] code)
        {
            module.Functions.Add(new Function { Type = typeIndex });
            module.Exports.Add(new Export { Name = name, Index = (uint)(module.Functions.Count - 1) });
            module.Codes.Add(new FunctionBody { Code = code });
        }

        AddFunction(nameof(Memory64Export.Load), 0,
            new LocalGet(0), new Int32Load(), new End());

        AddFunction(nameof(Memory64Export.LoadOffset16), 0,
            new LocalGet(0), new Int32Load { Offset64 = 16 }, new End());

        AddFunction(nameof(Memory64Export.Store), 1,
            new LocalGet(0), new LocalGet(1), new Int32Store(), new End());

        AddFunction(nameof(Memory64Export.Size), 2,
            new MemorySize(), new End());

        AddFunction(nameof(Memory64Export.Grow), 3,
            new LocalGet(0), new MemoryGrow(), new End());

        return module.ToInstance<Memory64Export>();
    }

    /// <summary>A store and load through i64 addresses near the top of the first page round-trips correctly.</summary>
    [TestMethod]
    public void Memory64_StoreLoad_RoundTripsAtHighAddress()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // Address 60000 is within the single 65536-byte page (the last i32 starts at 65532).
        const long address = 60000;
        const int value = 0x0BAD_F00D;

        Assert.AreEqual(0, exports.Load(address));
        exports.Store(address, value);
        Assert.AreEqual(value, exports.Load(address));
    }

    /// <summary>An access just past the current memory size traps rather than reading out of bounds.</summary>
    [TestMethod]
    public void Memory64_AccessBeyondSize_Traps()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // The single page is 65536 bytes; the last valid i32 starts at 65532. 65533 would read past the end.
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.Load(65533));
    }

    /// <summary>
    /// A huge dynamic address that exceeds the memory size traps — it is never truncated to 32 bits (which would
    /// alias a valid low address).
    /// </summary>
    [TestMethod]
    public void Memory64_HugeAddress_Traps()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // 0x1_0000_0000 truncated to 32 bits would be 0 (a valid address); at full width it is far out of range.
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.Load(0x1_0000_0000L));
    }

    /// <summary>
    /// The effective address (dynamic address + static offset) is computed in 64-bit unsigned arithmetic and traps
    /// on overflow rather than wrapping to a small in-bounds value.
    /// </summary>
    [TestMethod]
    public void Memory64_EffectiveAddressOverflow_Traps()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // address = 0xFFFF_FFFF_FFFF_FFFF; + the static offset of 16 overflows u64. A wrap would land at address 15
        // (in bounds); honest 64-bit arithmetic traps instead.
        Assert.ThrowsExactly<System.OverflowException>(() => exports.LoadOffset16(-1));
    }

    /// <summary><c>memory.size</c> and <c>memory.grow</c> return i64 page counts, and a failed grow returns -1.</summary>
    [TestMethod]
    public void Memory64_SizeAndGrow_ReturnI64()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        Assert.AreEqual(1L, exports.Size());

        // Growing to the two-page maximum succeeds and returns the previous size.
        Assert.AreEqual(1L, exports.Grow(1));
        Assert.AreEqual(2L, exports.Size());

        // A further grow exceeds the maximum and returns -1 (as i64), leaving the size unchanged.
        Assert.AreEqual(-1L, exports.Grow(1));
        Assert.AreEqual(2L, exports.Size());

        // An astronomically large delta can never be allocated and also returns -1.
        Assert.AreEqual(-1L, exports.Grow(long.MaxValue));
    }

    /// <summary>A 64-bit memory serializes its limits with the i64 flag bit set and round-trips faithfully.</summary>
    [TestMethod]
    public void Memory64_Limits_RoundTripWithFlagBit()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 3) { Is64Bit = true });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        stream.Position = 0;

        var roundTripped = Module.ReadFromBinary(stream);
        Assert.AreEqual(1, roundTripped.Memories.Count);
        var memory = roundTripped.Memories[0];
        Assert.IsTrue(memory.Is64Bit);
        Assert.AreEqual(1u, memory.ResizableLimits.Minimum);
        Assert.AreEqual(3u, memory.ResizableLimits.Maximum);
        Assert.AreEqual(1UL, memory.ResizableLimits.Minimum64);
        Assert.AreEqual(3UL, memory.ResizableLimits.Maximum64);

        // The limits flag byte is 0x05: bit 0 (has-maximum) | bit 2 (i64).
        stream.Position = 0;
        var bytes = stream.ToArray();
        AssertContainsSubsequence(bytes, [0x05, 0x01, 0x03]);
    }

    /// <summary>
    /// A 64-bit memory that declares more than one page holds the full-width minimum and can exceed
    /// <see cref="uint.MaxValue"/> pages at the object-model level, even though such a memory could never be allocated.
    /// </summary>
    [TestMethod]
    public void Memory64_AstronomicalMinimum_ReadsWithoutTruncation()
    {
        var module = new Module();
        // 2^40 pages is far beyond anything allocatable but under the 2^48 memory64 limit; reading must not truncate.
        const ulong minimum = 1UL << 40;
        module.Memories.Add(new Memory { ResizableLimits = new ResizableLimits(minimum, null, true) });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        stream.Position = 0;

        var roundTripped = Module.ReadFromBinary(stream);
        Assert.IsTrue(roundTripped.Memories[0].Is64Bit);
        Assert.AreEqual(minimum, roundTripped.Memories[0].ResizableLimits.Minimum64);
    }

    /// <summary>A 32-bit memory continues to serialize with the pre-3.0 limits encoding (no i64 flag bit).</summary>
    [TestMethod]
    public void Memory32_Limits_OmitFlagBit()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 2));

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        var bytes = stream.ToArray();

        // 0x01 (has-maximum, no i64 bit), min 0x01, max 0x02.
        AssertContainsSubsequence(bytes, [0x01, 0x01, 0x02]);
        Assert.IsFalse(Module.ReadFromBinary(new MemoryStream(bytes)).Memories[0].Is64Bit);
    }

    /// <summary>Exposes a 32-bit memory 0 and a 64-bit memory 1 for a mixed <c>memory.copy</c>.</summary>
    public abstract class MixedCopyExport
    {
        /// <summary>Stores <paramref name="value"/> into 64-bit memory 1, copies it into 32-bit memory 0, returns memory 0's word.</summary>
        public abstract int CopyMemory1ToMemory0(int value);
    }

    /// <summary>
    /// A <c>memory.copy</c> between a 32-bit and a 64-bit memory takes a differently-typed operand per side — an i32
    /// destination, an i64 source, and an i32 length (the narrower of the two) — and moves the bytes correctly.
    /// </summary>
    [TestMethod]
    public void Memory64_MixedCopy_BetweenI32AndI64Memories()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));                       // memory 0: 32-bit
        module.Memories.Add(new Memory(1, 1) { Is64Bit = true });   // memory 1: 64-bit

        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = nameof(MixedCopyExport.CopyMemory1ToMemory0), Index = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // memory1[0] = value (i64 address, memory index 1)
                new Int64Constant(0),
                new LocalGet(0),
                new Int32Store { MemoryIndex = 1 },
                // memory.copy dst(mem0, i32)=0, src(mem1, i64)=0, len(i32)=4
                new Int32Constant(0),
                new Int64Constant(0),
                new Int32Constant(4),
                new MemoryCopy { DestinationMemoryIndex = 0, SourceMemoryIndex = 1 },
                // return memory0[0] (i32 address, memory index 0)
                new Int32Constant(0),
                new Int32Load { MemoryIndex = 0 },
                new End(),
            ],
        });

        using var instance = module.ToInstance<MixedCopyExport>();
        Assert.AreEqual(0x1357_9BDF, instance.Exports.CopyMemory1ToMemory0(0x1357_9BDF));
    }

    private static void AssertContainsSubsequence(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i + needle.Length <= haystack.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return;
        }

        Assert.Fail($"Expected byte subsequence [{string.Join(", ", needle)}] was not found.");
    }
}

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests that the v128 memory instructions (loads, stores, extension/splat loads, load/store-lane, and the
/// zero-extending loads) honor WebAssembly 3.0 memory64: an i64 address operand, a <c>varuint64</c> memarg offset,
/// and honest 64-bit bounds checking that traps on effective-address overflow rather than wrapping. No ratified
/// spec fixture combines SIMD with memory64, so these unit tests are the coverage.
/// </summary>
[TestClass]
public class Simd64Tests
{
    /// <summary>A v128 load/store surface over a 64-bit linear memory, addressed by i64.</summary>
    public abstract class Simd64Export
    {
        /// <summary>Writes the vector [1..16] to <paramref name="address"/> via <c>v128.store</c>.</summary>
        public abstract void StoreVec(long address);

        /// <summary>Loads a v128 from <paramref name="address"/> and returns byte lane 0.</summary>
        public abstract int LoadVecLane0(long address);

        /// <summary>Loads a v128 from <paramref name="address"/> and returns byte lane 15.</summary>
        public abstract int LoadVecLane15(long address);

        /// <summary><c>v128.load</c> with a static offset above <see cref="uint.MaxValue"/>.</summary>
        public abstract int LoadHugeOffset(long address);

        /// <summary><c>v128.store</c> with a static offset above <see cref="uint.MaxValue"/>.</summary>
        public abstract void StoreHugeOffset(long address);

        /// <summary><c>v128.load8x8_s</c> at <paramref name="address"/>, returning i16 lane 0.</summary>
        public abstract int Load8x8SignedLane0(long address);

        /// <summary><c>v128.load8_splat</c> at <paramref name="address"/>, returning byte lane 5.</summary>
        public abstract int Load8SplatLane5(long address);

        /// <summary><c>v128.load32_zero</c> at <paramref name="address"/>, returning i32 lane 0.</summary>
        public abstract int Load32ZeroLane0(long address);

        /// <summary><c>v128.load32_zero</c> at <paramref name="address"/>, returning i32 lane 1 (must be zero).</summary>
        public abstract int Load32ZeroLane1(long address);

        /// <summary><c>v128.load8_lane</c> into lane 3 of an all-0xEE vector; returns lane 3 (the loaded byte).</summary>
        public abstract int Load8LaneInto3(long address);

        /// <summary><c>v128.load8_lane</c> into lane 3 of an all-0xEE vector; returns lane 0 (untouched 0xEE).</summary>
        public abstract int Load8LaneKeepsLane0(long address);

        /// <summary><c>v128.store8_lane</c> writes lane 2 of [1..16] to <paramref name="address"/>; reads it back.</summary>
        public abstract int Store8LaneReadBack(long address);
    }

    // The vector [1..16] used throughout: byte i (0-based) holds value i + 1.
    private static byte[] Ramp16()
    {
        var bytes = new byte[16];
        for (var i = 0; i < 16; i++)
            bytes[i] = (byte)(i + 1);
        return bytes;
    }

    private const ulong HugeOffset = 0x1_0000_0000; // 2^32, one past the largest 32-bit offset.

    private static Instance<Simd64Export> CreateInstance()
    {
        var module = new Module();
        // A 64-bit memory of one page (65536 bytes).
        module.Memories.Add(new Memory(1, 1) { Is64Bit = true });

        // Type 0: (i64)->i32; type 1: (i64)->().
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64], Returns = [WebAssemblyValueType.Int32] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64] });

        void AddFunction(string name, uint typeIndex, params Instruction[] code)
        {
            module.Functions.Add(new Function { Type = typeIndex });
            module.Exports.Add(new Export { Name = name, Index = (uint)(module.Functions.Count - 1) });
            module.Codes.Add(new FunctionBody { Code = code });
        }

        AddFunction(nameof(Simd64Export.StoreVec), 1,
            new LocalGet(0), new V128Const { Value = Ramp16() }, new V128Store(), new End());

        AddFunction(nameof(Simd64Export.LoadVecLane0), 0,
            new LocalGet(0), new V128Load(), new Int8x16ExtractLaneUnsigned { LaneIndex = 0 }, new End());

        AddFunction(nameof(Simd64Export.LoadVecLane15), 0,
            new LocalGet(0), new V128Load(), new Int8x16ExtractLaneUnsigned { LaneIndex = 15 }, new End());

        AddFunction(nameof(Simd64Export.LoadHugeOffset), 0,
            new LocalGet(0), new V128Load { Offset64 = HugeOffset }, new Int8x16ExtractLaneUnsigned { LaneIndex = 0 }, new End());

        AddFunction(nameof(Simd64Export.StoreHugeOffset), 1,
            new LocalGet(0), new V128Const { Value = Ramp16() }, new V128Store { Offset64 = HugeOffset }, new End());

        AddFunction(nameof(Simd64Export.Load8x8SignedLane0), 0,
            new LocalGet(0), new V128Load8X8Signed(), new Int16x8ExtractLaneSigned { LaneIndex = 0 }, new End());

        AddFunction(nameof(Simd64Export.Load8SplatLane5), 0,
            new LocalGet(0), new V128Load8Splat(), new Int8x16ExtractLaneUnsigned { LaneIndex = 5 }, new End());

        AddFunction(nameof(Simd64Export.Load32ZeroLane0), 0,
            new LocalGet(0), new V128Load32Zero(), new Int32x4ExtractLane { LaneIndex = 0 }, new End());

        AddFunction(nameof(Simd64Export.Load32ZeroLane1), 0,
            new LocalGet(0), new V128Load32Zero(), new Int32x4ExtractLane { LaneIndex = 1 }, new End());

        var allEe = new byte[16];
        Array.Fill(allEe, (byte)0xEE);

        AddFunction(nameof(Simd64Export.Load8LaneInto3), 0,
            new LocalGet(0), new V128Const { Value = allEe }, new V128Load8Lane { LaneIndex = 3 },
            new Int8x16ExtractLaneUnsigned { LaneIndex = 3 }, new End());

        AddFunction(nameof(Simd64Export.Load8LaneKeepsLane0), 0,
            new LocalGet(0), new V128Const { Value = allEe }, new V128Load8Lane { LaneIndex = 3 },
            new Int8x16ExtractLaneUnsigned { LaneIndex = 0 }, new End());

        AddFunction(nameof(Simd64Export.Store8LaneReadBack), 0,
            new LocalGet(0), new V128Const { Value = Ramp16() }, new V128Store8Lane { LaneIndex = 2 },
            new LocalGet(0), new Int32Load8Unsigned(), new End());

        return module.ToInstance<Simd64Export>();
    }

    /// <summary>A v128 store then load through i64 addresses round-trips every byte at a low address.</summary>
    [TestMethod]
    public void Simd64_StoreLoad_RoundTripsAtLowAddress()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        const long address = 32;
        exports.StoreVec(address);
        Assert.AreEqual(1, exports.LoadVecLane0(address));
        Assert.AreEqual(16, exports.LoadVecLane15(address));
    }

    /// <summary>The extension, splat, and zero-extending loads all read correctly from an i64 address.</summary>
    [TestMethod]
    public void Simd64_ExtensionSplatZeroLoads_ReadFromI64Address()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        const long address = 48;
        exports.StoreVec(address); // bytes 1..16 at `address`.

        // load8x8_s sign-extends byte 0 (value 1) into i16 lane 0.
        Assert.AreEqual(1, exports.Load8x8SignedLane0(address));

        // load8_splat replicates byte 0 (value 1) across all lanes.
        Assert.AreEqual(1, exports.Load8SplatLane5(address));

        // load32_zero reads the little-endian i32 from bytes 1,2,3,4 into lane 0 and zeroes lane 1.
        Assert.AreEqual(0x04030201, exports.Load32ZeroLane0(address));
        Assert.AreEqual(0, exports.Load32ZeroLane1(address));
    }

    /// <summary>The load-lane and store-lane instructions target an i64 address, leaving other lanes intact.</summary>
    [TestMethod]
    public void Simd64_LoadStoreLane_UseI64Address()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        const long address = 64;
        exports.StoreVec(address); // byte 0 at `address` is 1.

        // load8_lane replaces lane 3 with the byte at `address` (1) and keeps the other lanes (0xEE = 238).
        Assert.AreEqual(1, exports.Load8LaneInto3(address));
        Assert.AreEqual(0xEE, exports.Load8LaneKeepsLane0(address));

        // store8_lane writes lane 2 of [1..16] (value 3) to a fresh address; the scalar read confirms it.
        const long storeAddress = 128;
        Assert.AreEqual(3, exports.Store8LaneReadBack(storeAddress));
    }

    /// <summary>
    /// A static offset above <see cref="uint.MaxValue"/> is legal on a memory64, but the resulting effective
    /// address is far past the sub-4-GiB memory, so both the load and store trap out of range rather than wrapping
    /// down to an in-bounds address.
    /// </summary>
    [TestMethod]
    public void Simd64_LargeStaticOffset_TrapsOutOfRange()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // address 0 + 2^32 = 2^32, past the one-page memory: out of range, not wrapped to 0.
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.LoadHugeOffset(0));
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.StoreHugeOffset(0));
    }

    /// <summary>
    /// The effective address (i64 dynamic address + static offset) is computed in 64-bit unsigned arithmetic and
    /// traps on u64 overflow rather than wrapping to a small in-bounds value.
    /// </summary>
    [TestMethod]
    public void Simd64_EffectiveAddressOverflow_Traps()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // address 0xFFFF_FFFF_FFFF_FFFF + the 2^32 static offset overflows u64; a wrap would land in bounds.
        Assert.ThrowsExactly<OverflowException>(() => exports.LoadHugeOffset(-1));
        Assert.ThrowsExactly<OverflowException>(() => exports.StoreHugeOffset(-1));
    }

    /// <summary>A v128 access one byte past the end of the memory traps rather than reading/writing out of bounds.</summary>
    [TestMethod]
    public void Simd64_AccessAtMemoryEnd_Traps()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // The single page is 65536 bytes; the last valid 16-byte v128 access starts at 65520. 65521 runs past the end.
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.LoadVecLane0(65521));
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.StoreVec(65521));

        // Likewise for the lane store, whose 1-byte access is only valid through the final byte (65535).
        Assert.ThrowsExactly<MemoryAccessOutOfRangeException>(() => exports.Store8LaneReadBack(65536));
    }

    /// <summary>
    /// A module whose v128 access carries a 64-bit memarg offset (above <see cref="uint.MaxValue"/>) serializes and
    /// deserializes with the full offset preserved and re-serializes byte-for-byte identically.
    /// </summary>
    [TestMethod]
    public void Simd64_ModuleWith64BitOffset_RoundTripsByteIdentical()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1) { Is64Bit = true });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = "Load", Index = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new V128Load { Offset64 = HugeOffset },
                new Int8x16ExtractLaneUnsigned { LaneIndex = 0 },
                new End(),
            ],
        });

        using var first = new MemoryStream();
        module.WriteToBinary(first);
        var firstBytes = first.ToArray();

        first.Position = 0;
        var roundTripped = Module.ReadFromBinary(first);

        // The 64-bit offset survives the read; the 32-bit narrowing view would overflow on this value.
        var load = (V128Load)roundTripped.Codes[0].Code[1];
        Assert.AreEqual(HugeOffset, load.Offset64);
        Assert.ThrowsExactly<OverflowException>(() => _ = load.Offset);

        using var second = new MemoryStream();
        roundTripped.WriteToBinary(second);
        CollectionAssert.AreEqual(firstBytes, second.ToArray());
    }

    /// <summary>
    /// A 32-bit-range offset still encodes with the pre-3.0 minimal LEB128 (identical to the historical u32 path),
    /// so existing SIMD modules round-trip byte-for-byte through the widened ulong offset storage.
    /// </summary>
    [TestMethod]
    public void Simd64_SmallOffset_EncodesIdenticallyToLegacy()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = "Load", Index = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new V128Load { Offset = 0x1234 },
                new Int8x16ExtractLaneUnsigned { LaneIndex = 0 },
                new End(),
            ],
        });

        using var first = new MemoryStream();
        module.WriteToBinary(first);
        var firstBytes = first.ToArray();

        first.Position = 0;
        var roundTripped = Module.ReadFromBinary(first);
        var load = (V128Load)roundTripped.Codes[0].Code[1];
        Assert.AreEqual(0x1234u, load.Offset);
        Assert.AreEqual(0x1234UL, load.Offset64);

        using var second = new MemoryStream();
        roundTripped.WriteToBinary(second);
        CollectionAssert.AreEqual(firstBytes, second.ToArray());
    }

    /// <summary>
    /// The <see cref="SimdMemoryImmediateInstruction.Offset"/> narrowing view mirrors the scalar
    /// <c>MemoryImmediateInstruction</c>: it exposes a 64-bit value that fits in a <see cref="uint"/> and throws
    /// <see cref="OverflowException"/> for one that does not, while <see cref="SimdMemoryImmediateInstruction.Offset64"/>
    /// always reflects the full value.
    /// </summary>
    [TestMethod]
    public void Simd64_OffsetNarrowingView_Behaves()
    {
        // Setting the 32-bit view populates the 64-bit storage.
        var byOffset = new V128Load { Offset = 0xDEAD_BEEF };
        Assert.AreEqual(0xDEAD_BEEFu, byOffset.Offset);
        Assert.AreEqual(0xDEAD_BEEFUL, byOffset.Offset64);

        // A value beyond uint.MaxValue is readable only through Offset64; the narrow view overflows.
        var byOffset64 = new V128Load { Offset64 = (ulong)uint.MaxValue + 1 };
        Assert.AreEqual((ulong)uint.MaxValue + 1, byOffset64.Offset64);
        Assert.ThrowsExactly<OverflowException>(() => _ = byOffset64.Offset);

        // The exact boundary value still fits the narrow view.
        var atMax = new V128Load { Offset64 = uint.MaxValue };
        Assert.AreEqual(uint.MaxValue, atMax.Offset);
    }

    /// <summary>The lane-carrying subclass exposes the same 64-bit offset and equates on the full-width value.</summary>
    [TestMethod]
    public void Simd64_LaneInstruction_UsesFullWidthOffsetForEquality()
    {
        var a = new V128Load8Lane { Offset64 = HugeOffset, LaneIndex = 3 };
        var b = new V128Load8Lane { Offset64 = HugeOffset, LaneIndex = 3 };
        var different = new V128Load8Lane { Offset64 = HugeOffset + 1, LaneIndex = 3 };

        // Equality and hashing read Offset64 directly, so a >uint.MaxValue offset never trips the narrowing view.
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        Assert.AreNotEqual(a, different);
    }
}

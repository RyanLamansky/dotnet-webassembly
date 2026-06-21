using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebAssembly;

/// <summary>
/// Binary SIMD operation codes (sub-opcodes following the 0xFD prefix byte).
/// </summary>
public enum SimdOpCode : uint
{
    /// <summary>Load a v128 from memory.</summary>
    [OpCodeCharacteristics("v128.load")]
    V128Load = 0x00,

    /// <summary>Load 8 bytes, sign-extend each to i16, splat to i16x8.</summary>
    [OpCodeCharacteristics("v128.load8x8_s")]
    V128Load8X8Signed = 0x01,

    /// <summary>Load 8 bytes, zero-extend each to i16, splat to i16x8.</summary>
    [OpCodeCharacteristics("v128.load8x8_u")]
    V128Load8X8Unsigned = 0x02,

    /// <summary>Load 4 i16 lanes, sign-extend to i32x4.</summary>
    [OpCodeCharacteristics("v128.load16x4_s")]
    V128Load16X4Signed = 0x03,

    /// <summary>Load 4 i16 lanes, zero-extend to i32x4.</summary>
    [OpCodeCharacteristics("v128.load16x4_u")]
    V128Load16X4Unsigned = 0x04,

    /// <summary>Load 2 i32 lanes, sign-extend to i64x2.</summary>
    [OpCodeCharacteristics("v128.load32x2_s")]
    V128Load32X2Signed = 0x05,

    /// <summary>Load 2 i32 lanes, zero-extend to i64x2.</summary>
    [OpCodeCharacteristics("v128.load32x2_u")]
    V128Load32X2Unsigned = 0x06,

    /// <summary>Load a single byte, splat to all i8x16 lanes.</summary>
    [OpCodeCharacteristics("v128.load8_splat")]
    V128Load8Splat = 0x07,

    /// <summary>Load a single i16, splat to all i16x8 lanes.</summary>
    [OpCodeCharacteristics("v128.load16_splat")]
    V128Load16Splat = 0x08,

    /// <summary>Load a single i32, splat to all i32x4 lanes.</summary>
    [OpCodeCharacteristics("v128.load32_splat")]
    V128Load32Splat = 0x09,

    /// <summary>Load a single i64, splat to all i64x2 lanes.</summary>
    [OpCodeCharacteristics("v128.load64_splat")]
    V128Load64Splat = 0x0A,

    /// <summary>Store a v128 to memory.</summary>
    [OpCodeCharacteristics("v128.store")]
    V128Store = 0x0B,

    /// <summary>Produce a v128 from 16 immediate bytes.</summary>
    [OpCodeCharacteristics("v128.const")]
    V128Const = 0x0C,

    /// <summary>Shuffle two i8x16 vectors using a 16-byte lane index immediate.</summary>
    [OpCodeCharacteristics("i8x16.shuffle")]
    Int8x16Shuffle = 0x0D,

    /// <summary>Swizzle i8x16 lanes according to an index vector.</summary>
    [OpCodeCharacteristics("i8x16.swizzle")]
    Int8x16Swizzle = 0x0E,

    /// <summary>Splat an i32 to all i8x16 lanes.</summary>
    [OpCodeCharacteristics("i8x16.splat")]
    Int8x16Splat = 0x0F,

    /// <summary>Splat an i32 to all i16x8 lanes.</summary>
    [OpCodeCharacteristics("i16x8.splat")]
    Int16x8Splat = 0x10,

    /// <summary>Splat an i32 to all i32x4 lanes.</summary>
    [OpCodeCharacteristics("i32x4.splat")]
    Int32x4Splat = 0x11,

    /// <summary>Splat an i64 to all i64x2 lanes.</summary>
    [OpCodeCharacteristics("i64x2.splat")]
    Int64x2Splat = 0x12,

    /// <summary>Splat an f32 to all f32x4 lanes.</summary>
    [OpCodeCharacteristics("f32x4.splat")]
    Float32x4Splat = 0x13,

    /// <summary>Splat an f64 to all f64x2 lanes.</summary>
    [OpCodeCharacteristics("f64x2.splat")]
    Float64x2Splat = 0x14,

    /// <summary>Extract a signed i8 lane as i32.</summary>
    [OpCodeCharacteristics("i8x16.extract_lane_s")]
    Int8x16ExtractLaneSigned = 0x15,

    /// <summary>Extract an unsigned i8 lane as i32.</summary>
    [OpCodeCharacteristics("i8x16.extract_lane_u")]
    Int8x16ExtractLaneUnsigned = 0x16,

    /// <summary>Replace an i8x16 lane with an i32 value.</summary>
    [OpCodeCharacteristics("i8x16.replace_lane")]
    Int8x16ReplaceLane = 0x17,

    /// <summary>Extract a signed i16 lane as i32.</summary>
    [OpCodeCharacteristics("i16x8.extract_lane_s")]
    Int16x8ExtractLaneSigned = 0x18,

    /// <summary>Extract an unsigned i16 lane as i32.</summary>
    [OpCodeCharacteristics("i16x8.extract_lane_u")]
    Int16x8ExtractLaneUnsigned = 0x19,

    /// <summary>Replace an i16x8 lane with an i32 value.</summary>
    [OpCodeCharacteristics("i16x8.replace_lane")]
    Int16x8ReplaceLane = 0x1A,

    /// <summary>Extract an i32 lane.</summary>
    [OpCodeCharacteristics("i32x4.extract_lane")]
    Int32x4ExtractLane = 0x1B,

    /// <summary>Replace an i32x4 lane.</summary>
    [OpCodeCharacteristics("i32x4.replace_lane")]
    Int32x4ReplaceLane = 0x1C,

    /// <summary>Extract an i64 lane.</summary>
    [OpCodeCharacteristics("i64x2.extract_lane")]
    Int64x2ExtractLane = 0x1D,

    /// <summary>Replace an i64x2 lane.</summary>
    [OpCodeCharacteristics("i64x2.replace_lane")]
    Int64x2ReplaceLane = 0x1E,

    /// <summary>Extract an f32 lane.</summary>
    [OpCodeCharacteristics("f32x4.extract_lane")]
    Float32x4ExtractLane = 0x1F,

    /// <summary>Replace an f32x4 lane.</summary>
    [OpCodeCharacteristics("f32x4.replace_lane")]
    Float32x4ReplaceLane = 0x20,

    /// <summary>Extract an f64 lane.</summary>
    [OpCodeCharacteristics("f64x2.extract_lane")]
    Float64x2ExtractLane = 0x21,

    /// <summary>Replace an f64x2 lane.</summary>
    [OpCodeCharacteristics("f64x2.replace_lane")]
    Float64x2ReplaceLane = 0x22,

    // ---- i8x16 comparisons ----
    /// <summary>i8x16 equal.</summary>
    [OpCodeCharacteristics("i8x16.eq")]
    Int8x16Equal = 0x23,
    /// <summary>i8x16 not equal.</summary>
    [OpCodeCharacteristics("i8x16.ne")]
    Int8x16NotEqual = 0x24,
    /// <summary>i8x16 signed less-than.</summary>
    [OpCodeCharacteristics("i8x16.lt_s")]
    Int8x16LessThanSigned = 0x25,
    /// <summary>i8x16 unsigned less-than.</summary>
    [OpCodeCharacteristics("i8x16.lt_u")]
    Int8x16LessThanUnsigned = 0x26,
    /// <summary>i8x16 signed greater-than.</summary>
    [OpCodeCharacteristics("i8x16.gt_s")]
    Int8x16GreaterThanSigned = 0x27,
    /// <summary>i8x16 unsigned greater-than.</summary>
    [OpCodeCharacteristics("i8x16.gt_u")]
    Int8x16GreaterThanUnsigned = 0x28,
    /// <summary>i8x16 signed less-than-or-equal.</summary>
    [OpCodeCharacteristics("i8x16.le_s")]
    Int8x16LessThanOrEqualSigned = 0x29,
    /// <summary>i8x16 unsigned less-than-or-equal.</summary>
    [OpCodeCharacteristics("i8x16.le_u")]
    Int8x16LessThanOrEqualUnsigned = 0x2A,
    /// <summary>i8x16 signed greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i8x16.ge_s")]
    Int8x16GreaterThanOrEqualSigned = 0x2B,
    /// <summary>i8x16 unsigned greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i8x16.ge_u")]
    Int8x16GreaterThanOrEqualUnsigned = 0x2C,

    // ---- i16x8 comparisons ----
    /// <summary>i16x8 equal.</summary>
    [OpCodeCharacteristics("i16x8.eq")]
    Int16x8Equal = 0x2D,
    /// <summary>i16x8 not equal.</summary>
    [OpCodeCharacteristics("i16x8.ne")]
    Int16x8NotEqual = 0x2E,
    /// <summary>i16x8 signed less-than.</summary>
    [OpCodeCharacteristics("i16x8.lt_s")]
    Int16x8LessThanSigned = 0x2F,
    /// <summary>i16x8 unsigned less-than.</summary>
    [OpCodeCharacteristics("i16x8.lt_u")]
    Int16x8LessThanUnsigned = 0x30,
    /// <summary>i16x8 signed greater-than.</summary>
    [OpCodeCharacteristics("i16x8.gt_s")]
    Int16x8GreaterThanSigned = 0x31,
    /// <summary>i16x8 unsigned greater-than.</summary>
    [OpCodeCharacteristics("i16x8.gt_u")]
    Int16x8GreaterThanUnsigned = 0x32,
    /// <summary>i16x8 signed less-than-or-equal.</summary>
    [OpCodeCharacteristics("i16x8.le_s")]
    Int16x8LessThanOrEqualSigned = 0x33,
    /// <summary>i16x8 unsigned less-than-or-equal.</summary>
    [OpCodeCharacteristics("i16x8.le_u")]
    Int16x8LessThanOrEqualUnsigned = 0x34,
    /// <summary>i16x8 signed greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i16x8.ge_s")]
    Int16x8GreaterThanOrEqualSigned = 0x35,
    /// <summary>i16x8 unsigned greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i16x8.ge_u")]
    Int16x8GreaterThanOrEqualUnsigned = 0x36,

    // ---- i32x4 comparisons ----
    /// <summary>i32x4 equal.</summary>
    [OpCodeCharacteristics("i32x4.eq")]
    Int32x4Equal = 0x37,
    /// <summary>i32x4 not equal.</summary>
    [OpCodeCharacteristics("i32x4.ne")]
    Int32x4NotEqual = 0x38,
    /// <summary>i32x4 signed less-than.</summary>
    [OpCodeCharacteristics("i32x4.lt_s")]
    Int32x4LessThanSigned = 0x39,
    /// <summary>i32x4 unsigned less-than.</summary>
    [OpCodeCharacteristics("i32x4.lt_u")]
    Int32x4LessThanUnsigned = 0x3A,
    /// <summary>i32x4 signed greater-than.</summary>
    [OpCodeCharacteristics("i32x4.gt_s")]
    Int32x4GreaterThanSigned = 0x3B,
    /// <summary>i32x4 unsigned greater-than.</summary>
    [OpCodeCharacteristics("i32x4.gt_u")]
    Int32x4GreaterThanUnsigned = 0x3C,
    /// <summary>i32x4 signed less-than-or-equal.</summary>
    [OpCodeCharacteristics("i32x4.le_s")]
    Int32x4LessThanOrEqualSigned = 0x3D,
    /// <summary>i32x4 unsigned less-than-or-equal.</summary>
    [OpCodeCharacteristics("i32x4.le_u")]
    Int32x4LessThanOrEqualUnsigned = 0x3E,
    /// <summary>i32x4 signed greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i32x4.ge_s")]
    Int32x4GreaterThanOrEqualSigned = 0x3F,
    /// <summary>i32x4 unsigned greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i32x4.ge_u")]
    Int32x4GreaterThanOrEqualUnsigned = 0x40,

    // ---- f32x4 comparisons ----
    /// <summary>f32x4 equal.</summary>
    [OpCodeCharacteristics("f32x4.eq")]
    Float32x4Equal = 0x41,
    /// <summary>f32x4 not equal.</summary>
    [OpCodeCharacteristics("f32x4.ne")]
    Float32x4NotEqual = 0x42,
    /// <summary>f32x4 less-than.</summary>
    [OpCodeCharacteristics("f32x4.lt")]
    Float32x4LessThan = 0x43,
    /// <summary>f32x4 greater-than.</summary>
    [OpCodeCharacteristics("f32x4.gt")]
    Float32x4GreaterThan = 0x44,
    /// <summary>f32x4 less-than-or-equal.</summary>
    [OpCodeCharacteristics("f32x4.le")]
    Float32x4LessThanOrEqual = 0x45,
    /// <summary>f32x4 greater-than-or-equal.</summary>
    [OpCodeCharacteristics("f32x4.ge")]
    Float32x4GreaterThanOrEqual = 0x46,

    // ---- f64x2 comparisons ----
    /// <summary>f64x2 equal.</summary>
    [OpCodeCharacteristics("f64x2.eq")]
    Float64x2Equal = 0x47,
    /// <summary>f64x2 not equal.</summary>
    [OpCodeCharacteristics("f64x2.ne")]
    Float64x2NotEqual = 0x48,
    /// <summary>f64x2 less-than.</summary>
    [OpCodeCharacteristics("f64x2.lt")]
    Float64x2LessThan = 0x49,
    /// <summary>f64x2 greater-than.</summary>
    [OpCodeCharacteristics("f64x2.gt")]
    Float64x2GreaterThan = 0x4A,
    /// <summary>f64x2 less-than-or-equal.</summary>
    [OpCodeCharacteristics("f64x2.le")]
    Float64x2LessThanOrEqual = 0x4B,
    /// <summary>f64x2 greater-than-or-equal.</summary>
    [OpCodeCharacteristics("f64x2.ge")]
    Float64x2GreaterThanOrEqual = 0x4C,

    // ---- v128 bitwise ops ----
    /// <summary>Bitwise NOT.</summary>
    [OpCodeCharacteristics("v128.not")]
    V128Not = 0x4D,
    /// <summary>Bitwise AND.</summary>
    [OpCodeCharacteristics("v128.and")]
    V128And = 0x4E,
    /// <summary>Bitwise ANDNOT.</summary>
    [OpCodeCharacteristics("v128.andnot")]
    V128AndNot = 0x4F,
    /// <summary>Bitwise OR.</summary>
    [OpCodeCharacteristics("v128.or")]
    V128Or = 0x50,
    /// <summary>Bitwise XOR.</summary>
    [OpCodeCharacteristics("v128.xor")]
    V128Xor = 0x51,
    /// <summary>Bitwise select.</summary>
    [OpCodeCharacteristics("v128.bitselect")]
    V128Bitselect = 0x52,
    /// <summary>Test whether any lane is non-zero.</summary>
    [OpCodeCharacteristics("v128.any_true")]
    V128AnyTrue = 0x53,

    // ---- v128.load/store lane ----
    /// <summary>Load and insert a single i8 lane.</summary>
    [OpCodeCharacteristics("v128.load8_lane")]
    V128Load8Lane = 0x54,
    /// <summary>Load and insert a single i16 lane.</summary>
    [OpCodeCharacteristics("v128.load16_lane")]
    V128Load16Lane = 0x55,
    /// <summary>Load and insert a single i32 lane.</summary>
    [OpCodeCharacteristics("v128.load32_lane")]
    V128Load32Lane = 0x56,
    /// <summary>Load and insert a single i64 lane.</summary>
    [OpCodeCharacteristics("v128.load64_lane")]
    V128Load64Lane = 0x57,
    /// <summary>Store a single i8 lane to memory.</summary>
    [OpCodeCharacteristics("v128.store8_lane")]
    V128Store8Lane = 0x58,
    /// <summary>Store a single i16 lane to memory.</summary>
    [OpCodeCharacteristics("v128.store16_lane")]
    V128Store16Lane = 0x59,
    /// <summary>Store a single i32 lane to memory.</summary>
    [OpCodeCharacteristics("v128.store32_lane")]
    V128Store32Lane = 0x5A,
    /// <summary>Store a single i64 lane to memory.</summary>
    [OpCodeCharacteristics("v128.store64_lane")]
    V128Store64Lane = 0x5B,
    /// <summary>Load 32 bits, zero-extend to v128.</summary>
    [OpCodeCharacteristics("v128.load32_zero")]
    V128Load32Zero = 0x5C,
    /// <summary>Load 64 bits, zero-extend to v128.</summary>
    [OpCodeCharacteristics("v128.load64_zero")]
    V128Load64Zero = 0x5D,

    // ---- f32x4/f64x2 convert ----
    /// <summary>Convert f32x4 to f64x2 (low two lanes).</summary>
    [OpCodeCharacteristics("f64x2.promote_low_f32x4")]
    Float64x2PromoteLowFloat32x4 = 0x5F,
    /// <summary>Demote f64x2 to f32x4.</summary>
    [OpCodeCharacteristics("f32x4.demote_f64x2_zero")]
    Float32x4DemoteFloat64x2Zero = 0x5E,

    // ---- i8x16 ops ----
    /// <summary>i8x16 absolute value.</summary>
    [OpCodeCharacteristics("i8x16.abs")]
    Int8x16Abs = 0x60,
    /// <summary>i8x16 negate.</summary>
    [OpCodeCharacteristics("i8x16.neg")]
    Int8x16Neg = 0x61,
    /// <summary>Count non-zero bits in each i8x16 lane.</summary>
    [OpCodeCharacteristics("i8x16.popcnt")]
    Int8x16Popcnt = 0x62,
    /// <summary>Test whether all i8x16 lanes are non-zero.</summary>
    [OpCodeCharacteristics("i8x16.all_true")]
    Int8x16AllTrue = 0x63,
    /// <summary>Bitmask of MSB of each i8x16 lane.</summary>
    [OpCodeCharacteristics("i8x16.bitmask")]
    Int8x16Bitmask = 0x64,
    /// <summary>Narrow i16x8 to i8x16, signed with saturation.</summary>
    [OpCodeCharacteristics("i8x16.narrow_i16x8_s")]
    Int8x16NarrowInt16x8Signed = 0x65,
    /// <summary>Narrow i16x8 to i8x16, unsigned with saturation.</summary>
    [OpCodeCharacteristics("i8x16.narrow_i16x8_u")]
    Int8x16NarrowInt16x8Unsigned = 0x66,
    /// <summary>i8x16 shift left.</summary>
    [OpCodeCharacteristics("i8x16.shl")]
    Int8x16ShiftLeft = 0x6B,
    /// <summary>i8x16 signed shift right.</summary>
    [OpCodeCharacteristics("i8x16.shr_s")]
    Int8x16ShiftRightSigned = 0x6C,
    /// <summary>i8x16 unsigned shift right.</summary>
    [OpCodeCharacteristics("i8x16.shr_u")]
    Int8x16ShiftRightUnsigned = 0x6D,
    /// <summary>i8x16 add.</summary>
    [OpCodeCharacteristics("i8x16.add")]
    Int8x16Add = 0x6E,
    /// <summary>i8x16 signed add with saturation.</summary>
    [OpCodeCharacteristics("i8x16.add_sat_s")]
    Int8x16AddSaturateSigned = 0x6F,
    /// <summary>i8x16 unsigned add with saturation.</summary>
    [OpCodeCharacteristics("i8x16.add_sat_u")]
    Int8x16AddSaturateUnsigned = 0x70,
    /// <summary>i8x16 subtract.</summary>
    [OpCodeCharacteristics("i8x16.sub")]
    Int8x16Sub = 0x71,
    /// <summary>i8x16 signed subtract with saturation.</summary>
    [OpCodeCharacteristics("i8x16.sub_sat_s")]
    Int8x16SubSaturateSigned = 0x72,
    /// <summary>i8x16 unsigned subtract with saturation.</summary>
    [OpCodeCharacteristics("i8x16.sub_sat_u")]
    Int8x16SubSaturateUnsigned = 0x73,
    /// <summary>i8x16 signed min.</summary>
    [OpCodeCharacteristics("i8x16.min_s")]
    Int8x16MinSigned = 0x76,
    /// <summary>i8x16 unsigned min.</summary>
    [OpCodeCharacteristics("i8x16.min_u")]
    Int8x16MinUnsigned = 0x77,
    /// <summary>i8x16 signed max.</summary>
    [OpCodeCharacteristics("i8x16.max_s")]
    Int8x16MaxSigned = 0x78,
    /// <summary>i8x16 unsigned max.</summary>
    [OpCodeCharacteristics("i8x16.max_u")]
    Int8x16MaxUnsigned = 0x79,
    /// <summary>i8x16 unsigned average (rounding).</summary>
    [OpCodeCharacteristics("i8x16.avgr_u")]
    Int8x16AvgrUnsigned = 0x7B,

    // ---- i16x8 ops ----
    /// <summary>i16x8 pairwise add of adjacent i8x16 lanes, signed.</summary>
    [OpCodeCharacteristics("i16x8.extadd_pairwise_i8x16_s")]
    Int16x8ExtaddPairwiseInt8x16Signed = 0x7C,
    /// <summary>i16x8 pairwise add of adjacent i8x16 lanes, unsigned.</summary>
    [OpCodeCharacteristics("i16x8.extadd_pairwise_i8x16_u")]
    Int16x8ExtaddPairwiseInt8x16Unsigned = 0x7D,
    /// <summary>i16x8 absolute value.</summary>
    [OpCodeCharacteristics("i16x8.abs")]
    Int16x8Abs = 0x80,
    /// <summary>i16x8 negate.</summary>
    [OpCodeCharacteristics("i16x8.neg")]
    Int16x8Neg = 0x81,
    /// <summary>i16x8 saturating rounding Q15 multiplication.</summary>
    [OpCodeCharacteristics("i16x8.q15mulr_sat_s")]
    Int16x8Q15MulrSatSigned = 0x82,
    /// <summary>Test whether all i16x8 lanes are non-zero.</summary>
    [OpCodeCharacteristics("i16x8.all_true")]
    Int16x8AllTrue = 0x83,
    /// <summary>Bitmask of MSB of each i16x8 lane.</summary>
    [OpCodeCharacteristics("i16x8.bitmask")]
    Int16x8Bitmask = 0x84,
    /// <summary>Narrow i32x4 to i16x8, signed with saturation.</summary>
    [OpCodeCharacteristics("i16x8.narrow_i32x4_s")]
    Int16x8NarrowInt32x4Signed = 0x85,
    /// <summary>Narrow i32x4 to i16x8, unsigned with saturation.</summary>
    [OpCodeCharacteristics("i16x8.narrow_i32x4_u")]
    Int16x8NarrowInt32x4Unsigned = 0x86,
    /// <summary>Widen low i8x16 lanes to i16x8, signed.</summary>
    [OpCodeCharacteristics("i16x8.extend_low_i8x16_s")]
    Int16x8ExtendLowInt8x16Signed = 0x87,
    /// <summary>Widen high i8x16 lanes to i16x8, signed.</summary>
    [OpCodeCharacteristics("i16x8.extend_high_i8x16_s")]
    Int16x8ExtendHighInt8x16Signed = 0x88,
    /// <summary>Widen low i8x16 lanes to i16x8, unsigned.</summary>
    [OpCodeCharacteristics("i16x8.extend_low_i8x16_u")]
    Int16x8ExtendLowInt8x16Unsigned = 0x89,
    /// <summary>Widen high i8x16 lanes to i16x8, unsigned.</summary>
    [OpCodeCharacteristics("i16x8.extend_high_i8x16_u")]
    Int16x8ExtendHighInt8x16Unsigned = 0x8A,
    /// <summary>i16x8 shift left.</summary>
    [OpCodeCharacteristics("i16x8.shl")]
    Int16x8ShiftLeft = 0x8B,
    /// <summary>i16x8 signed shift right.</summary>
    [OpCodeCharacteristics("i16x8.shr_s")]
    Int16x8ShiftRightSigned = 0x8C,
    /// <summary>i16x8 unsigned shift right.</summary>
    [OpCodeCharacteristics("i16x8.shr_u")]
    Int16x8ShiftRightUnsigned = 0x8D,
    /// <summary>i16x8 add.</summary>
    [OpCodeCharacteristics("i16x8.add")]
    Int16x8Add = 0x8E,
    /// <summary>i16x8 signed add with saturation.</summary>
    [OpCodeCharacteristics("i16x8.add_sat_s")]
    Int16x8AddSaturateSigned = 0x8F,
    /// <summary>i16x8 unsigned add with saturation.</summary>
    [OpCodeCharacteristics("i16x8.add_sat_u")]
    Int16x8AddSaturateUnsigned = 0x90,
    /// <summary>i16x8 subtract.</summary>
    [OpCodeCharacteristics("i16x8.sub")]
    Int16x8Sub = 0x91,
    /// <summary>i16x8 signed subtract with saturation.</summary>
    [OpCodeCharacteristics("i16x8.sub_sat_s")]
    Int16x8SubSaturateSigned = 0x92,
    /// <summary>i16x8 unsigned subtract with saturation.</summary>
    [OpCodeCharacteristics("i16x8.sub_sat_u")]
    Int16x8SubSaturateUnsigned = 0x93,
    /// <summary>i16x8 multiply.</summary>
    [OpCodeCharacteristics("i16x8.mul")]
    Int16x8Mul = 0x95,
    /// <summary>i16x8 signed min.</summary>
    [OpCodeCharacteristics("i16x8.min_s")]
    Int16x8MinSigned = 0x96,
    /// <summary>i16x8 unsigned min.</summary>
    [OpCodeCharacteristics("i16x8.min_u")]
    Int16x8MinUnsigned = 0x97,
    /// <summary>i16x8 signed max.</summary>
    [OpCodeCharacteristics("i16x8.max_s")]
    Int16x8MaxSigned = 0x98,
    /// <summary>i16x8 unsigned max.</summary>
    [OpCodeCharacteristics("i16x8.max_u")]
    Int16x8MaxUnsigned = 0x99,
    /// <summary>i16x8 unsigned average (rounding).</summary>
    [OpCodeCharacteristics("i16x8.avgr_u")]
    Int16x8AvgrUnsigned = 0x9B,
    /// <summary>Multiply i16x8 lanes, accumulate to i32x4.</summary>
    [OpCodeCharacteristics("i16x8.extmul_low_i8x16_s")]
    Int16x8ExtmulLowInt8x16Signed = 0x9C,
    /// <summary>Multiply high i16x8 lanes, accumulate to i32x4.</summary>
    [OpCodeCharacteristics("i16x8.extmul_high_i8x16_s")]
    Int16x8ExtmulHighInt8x16Signed = 0x9D,
    /// <summary>Multiply low unsigned i16x8 lanes.</summary>
    [OpCodeCharacteristics("i16x8.extmul_low_i8x16_u")]
    Int16x8ExtmulLowInt8x16Unsigned = 0x9E,
    /// <summary>Multiply high unsigned i16x8 lanes.</summary>
    [OpCodeCharacteristics("i16x8.extmul_high_i8x16_u")]
    Int16x8ExtmulHighInt8x16Unsigned = 0x9F,

    // ---- i32x4 ops ----
    /// <summary>i32x4 pairwise add.</summary>
    [OpCodeCharacteristics("i32x4.extadd_pairwise_i16x8_s")]
    Int32x4ExtaddPairwiseInt16x8Signed = 0x7E,
    /// <summary>i32x4 pairwise add unsigned.</summary>
    [OpCodeCharacteristics("i32x4.extadd_pairwise_i16x8_u")]
    Int32x4ExtaddPairwiseInt16x8Unsigned = 0x7F,
    /// <summary>i32x4 absolute value.</summary>
    [OpCodeCharacteristics("i32x4.abs")]
    Int32x4Abs = 0xA0,
    /// <summary>i32x4 negate.</summary>
    [OpCodeCharacteristics("i32x4.neg")]
    Int32x4Neg = 0xA1,
    /// <summary>Test whether all i32x4 lanes are non-zero.</summary>
    [OpCodeCharacteristics("i32x4.all_true")]
    Int32x4AllTrue = 0xA3,
    /// <summary>Bitmask of MSB of each i32x4 lane.</summary>
    [OpCodeCharacteristics("i32x4.bitmask")]
    Int32x4Bitmask = 0xA4,
    /// <summary>Widen low i16x8 lanes to i32x4, signed.</summary>
    [OpCodeCharacteristics("i32x4.extend_low_i16x8_s")]
    Int32x4ExtendLowInt16x8Signed = 0xA7,
    /// <summary>Widen high i16x8 lanes to i32x4, signed.</summary>
    [OpCodeCharacteristics("i32x4.extend_high_i16x8_s")]
    Int32x4ExtendHighInt16x8Signed = 0xA8,
    /// <summary>Widen low i16x8 lanes to i32x4, unsigned.</summary>
    [OpCodeCharacteristics("i32x4.extend_low_i16x8_u")]
    Int32x4ExtendLowInt16x8Unsigned = 0xA9,
    /// <summary>Widen high i16x8 lanes to i32x4, unsigned.</summary>
    [OpCodeCharacteristics("i32x4.extend_high_i16x8_u")]
    Int32x4ExtendHighInt16x8Unsigned = 0xAA,
    /// <summary>i32x4 shift left.</summary>
    [OpCodeCharacteristics("i32x4.shl")]
    Int32x4ShiftLeft = 0xAB,
    /// <summary>i32x4 signed shift right.</summary>
    [OpCodeCharacteristics("i32x4.shr_s")]
    Int32x4ShiftRightSigned = 0xAC,
    /// <summary>i32x4 unsigned shift right.</summary>
    [OpCodeCharacteristics("i32x4.shr_u")]
    Int32x4ShiftRightUnsigned = 0xAD,
    /// <summary>i32x4 add.</summary>
    [OpCodeCharacteristics("i32x4.add")]
    Int32x4Add = 0xAE,
    /// <summary>i32x4 subtract.</summary>
    [OpCodeCharacteristics("i32x4.sub")]
    Int32x4Sub = 0xB1,
    /// <summary>i32x4 multiply.</summary>
    [OpCodeCharacteristics("i32x4.mul")]
    Int32x4Mul = 0xB5,
    /// <summary>i32x4 signed min.</summary>
    [OpCodeCharacteristics("i32x4.min_s")]
    Int32x4MinSigned = 0xB6,
    /// <summary>i32x4 unsigned min.</summary>
    [OpCodeCharacteristics("i32x4.min_u")]
    Int32x4MinUnsigned = 0xB7,
    /// <summary>i32x4 signed max.</summary>
    [OpCodeCharacteristics("i32x4.max_s")]
    Int32x4MaxSigned = 0xB8,
    /// <summary>i32x4 unsigned max.</summary>
    [OpCodeCharacteristics("i32x4.max_u")]
    Int32x4MaxUnsigned = 0xB9,
    /// <summary>i32x4 unsigned dot product of i16x8.</summary>
    [OpCodeCharacteristics("i32x4.dot_i16x8_s")]
    Int32x4DotInt16x8Signed = 0xBA,
    /// <summary>Multiply low i16x8 lanes, extend to i32x4, signed.</summary>
    [OpCodeCharacteristics("i32x4.extmul_low_i16x8_s")]
    Int32x4ExtmulLowInt16x8Signed = 0xBC,
    /// <summary>Multiply high i16x8 lanes, extend to i32x4, signed.</summary>
    [OpCodeCharacteristics("i32x4.extmul_high_i16x8_s")]
    Int32x4ExtmulHighInt16x8Signed = 0xBD,
    /// <summary>Multiply low i16x8 lanes, extend to i32x4, unsigned.</summary>
    [OpCodeCharacteristics("i32x4.extmul_low_i16x8_u")]
    Int32x4ExtmulLowInt16x8Unsigned = 0xBE,
    /// <summary>Multiply high i16x8 lanes, extend to i32x4, unsigned.</summary>
    [OpCodeCharacteristics("i32x4.extmul_high_i16x8_u")]
    Int32x4ExtmulHighInt16x8Unsigned = 0xBF,

    // ---- i64x2 ops ----
    /// <summary>i64x2 absolute value.</summary>
    [OpCodeCharacteristics("i64x2.abs")]
    Int64x2Abs = 0xC0,
    /// <summary>i64x2 negate.</summary>
    [OpCodeCharacteristics("i64x2.neg")]
    Int64x2Neg = 0xC1,
    /// <summary>Test whether all i64x2 lanes are non-zero.</summary>
    [OpCodeCharacteristics("i64x2.all_true")]
    Int64x2AllTrue = 0xC3,
    /// <summary>Bitmask of MSB of each i64x2 lane.</summary>
    [OpCodeCharacteristics("i64x2.bitmask")]
    Int64x2Bitmask = 0xC4,
    /// <summary>Widen low i32x4 lanes to i64x2, signed.</summary>
    [OpCodeCharacteristics("i64x2.extend_low_i32x4_s")]
    Int64x2ExtendLowInt32x4Signed = 0xC7,
    /// <summary>Widen high i32x4 lanes to i64x2, signed.</summary>
    [OpCodeCharacteristics("i64x2.extend_high_i32x4_s")]
    Int64x2ExtendHighInt32x4Signed = 0xC8,
    /// <summary>Widen low i32x4 lanes to i64x2, unsigned.</summary>
    [OpCodeCharacteristics("i64x2.extend_low_i32x4_u")]
    Int64x2ExtendLowInt32x4Unsigned = 0xC9,
    /// <summary>Widen high i32x4 lanes to i64x2, unsigned.</summary>
    [OpCodeCharacteristics("i64x2.extend_high_i32x4_u")]
    Int64x2ExtendHighInt32x4Unsigned = 0xCA,
    /// <summary>i64x2 shift left.</summary>
    [OpCodeCharacteristics("i64x2.shl")]
    Int64x2ShiftLeft = 0xCB,
    /// <summary>i64x2 signed shift right.</summary>
    [OpCodeCharacteristics("i64x2.shr_s")]
    Int64x2ShiftRightSigned = 0xCC,
    /// <summary>i64x2 unsigned shift right.</summary>
    [OpCodeCharacteristics("i64x2.shr_u")]
    Int64x2ShiftRightUnsigned = 0xCD,
    /// <summary>i64x2 add.</summary>
    [OpCodeCharacteristics("i64x2.add")]
    Int64x2Add = 0xCE,
    /// <summary>i64x2 subtract.</summary>
    [OpCodeCharacteristics("i64x2.sub")]
    Int64x2Sub = 0xD1,
    /// <summary>i64x2 multiply.</summary>
    [OpCodeCharacteristics("i64x2.mul")]
    Int64x2Mul = 0xD5,
    /// <summary>i64x2 equal.</summary>
    [OpCodeCharacteristics("i64x2.eq")]
    Int64x2Equal = 0xD6,
    /// <summary>i64x2 not equal.</summary>
    [OpCodeCharacteristics("i64x2.ne")]
    Int64x2NotEqual = 0xD7,
    /// <summary>i64x2 signed less-than.</summary>
    [OpCodeCharacteristics("i64x2.lt_s")]
    Int64x2LessThanSigned = 0xD8,
    /// <summary>i64x2 signed greater-than.</summary>
    [OpCodeCharacteristics("i64x2.gt_s")]
    Int64x2GreaterThanSigned = 0xD9,
    /// <summary>i64x2 signed less-than-or-equal.</summary>
    [OpCodeCharacteristics("i64x2.le_s")]
    Int64x2LessThanOrEqualSigned = 0xDA,
    /// <summary>i64x2 signed greater-than-or-equal.</summary>
    [OpCodeCharacteristics("i64x2.ge_s")]
    Int64x2GreaterThanOrEqualSigned = 0xDB,
    /// <summary>Multiply i64x2 lanes, signed extended from low i32x4.</summary>
    [OpCodeCharacteristics("i64x2.extmul_low_i32x4_s")]
    Int64x2ExtmulLowInt32x4Signed = 0xDC,
    /// <summary>Multiply i64x2 lanes, signed extended from high i32x4.</summary>
    [OpCodeCharacteristics("i64x2.extmul_high_i32x4_s")]
    Int64x2ExtmulHighInt32x4Signed = 0xDD,
    /// <summary>Multiply i64x2 lanes, unsigned extended from low i32x4.</summary>
    [OpCodeCharacteristics("i64x2.extmul_low_i32x4_u")]
    Int64x2ExtmulLowInt32x4Unsigned = 0xDE,
    /// <summary>Multiply i64x2 lanes, unsigned extended from high i32x4.</summary>
    [OpCodeCharacteristics("i64x2.extmul_high_i32x4_u")]
    Int64x2ExtmulHighInt32x4Unsigned = 0xDF,

    // ---- f32x4 ops ----
    /// <summary>f32x4 ceiling.</summary>
    [OpCodeCharacteristics("f32x4.ceil")]
    Float32x4Ceil = 0x67,
    /// <summary>f32x4 floor.</summary>
    [OpCodeCharacteristics("f32x4.floor")]
    Float32x4Floor = 0x68,
    /// <summary>f32x4 truncate.</summary>
    [OpCodeCharacteristics("f32x4.trunc")]
    Float32x4Trunc = 0x69,
    /// <summary>f32x4 nearest integer.</summary>
    [OpCodeCharacteristics("f32x4.nearest")]
    Float32x4Nearest = 0x6A,
    /// <summary>f32x4 absolute value.</summary>
    [OpCodeCharacteristics("f32x4.abs")]
    Float32x4Abs = 0xE0,
    /// <summary>f32x4 negate.</summary>
    [OpCodeCharacteristics("f32x4.neg")]
    Float32x4Neg = 0xE1,
    /// <summary>f32x4 square root.</summary>
    [OpCodeCharacteristics("f32x4.sqrt")]
    Float32x4Sqrt = 0xE3,
    /// <summary>f32x4 add.</summary>
    [OpCodeCharacteristics("f32x4.add")]
    Float32x4Add = 0xE4,
    /// <summary>f32x4 subtract.</summary>
    [OpCodeCharacteristics("f32x4.sub")]
    Float32x4Sub = 0xE5,
    /// <summary>f32x4 multiply.</summary>
    [OpCodeCharacteristics("f32x4.mul")]
    Float32x4Mul = 0xE6,
    /// <summary>f32x4 divide.</summary>
    [OpCodeCharacteristics("f32x4.div")]
    Float32x4Div = 0xE7,
    /// <summary>f32x4 min.</summary>
    [OpCodeCharacteristics("f32x4.min")]
    Float32x4Min = 0xE8,
    /// <summary>f32x4 max.</summary>
    [OpCodeCharacteristics("f32x4.max")]
    Float32x4Max = 0xE9,
    /// <summary>f32x4 pseudo-min.</summary>
    [OpCodeCharacteristics("f32x4.pmin")]
    Float32x4Pmin = 0xEA,
    /// <summary>f32x4 pseudo-max.</summary>
    [OpCodeCharacteristics("f32x4.pmax")]
    Float32x4Pmax = 0xEB,

    // ---- f64x2 ops ----
    /// <summary>f64x2 ceiling.</summary>
    [OpCodeCharacteristics("f64x2.ceil")]
    Float64x2Ceil = 0x74,
    /// <summary>f64x2 floor.</summary>
    [OpCodeCharacteristics("f64x2.floor")]
    Float64x2Floor = 0x75,
    /// <summary>f64x2 truncate.</summary>
    [OpCodeCharacteristics("f64x2.trunc")]
    Float64x2Trunc = 0x7A,
    /// <summary>f64x2 nearest integer.</summary>
    [OpCodeCharacteristics("f64x2.nearest")]
    Float64x2Nearest = 0x94,
    /// <summary>f64x2 absolute value.</summary>
    [OpCodeCharacteristics("f64x2.abs")]
    Float64x2Abs = 0xEC,
    /// <summary>f64x2 negate.</summary>
    [OpCodeCharacteristics("f64x2.neg")]
    Float64x2Neg = 0xED,
    /// <summary>f64x2 square root.</summary>
    [OpCodeCharacteristics("f64x2.sqrt")]
    Float64x2Sqrt = 0xEF,
    /// <summary>f64x2 add.</summary>
    [OpCodeCharacteristics("f64x2.add")]
    Float64x2Add = 0xF0,
    /// <summary>f64x2 subtract.</summary>
    [OpCodeCharacteristics("f64x2.sub")]
    Float64x2Sub = 0xF1,
    /// <summary>f64x2 multiply.</summary>
    [OpCodeCharacteristics("f64x2.mul")]
    Float64x2Mul = 0xF2,
    /// <summary>f64x2 divide.</summary>
    [OpCodeCharacteristics("f64x2.div")]
    Float64x2Div = 0xF3,
    /// <summary>f64x2 min.</summary>
    [OpCodeCharacteristics("f64x2.min")]
    Float64x2Min = 0xF4,
    /// <summary>f64x2 max.</summary>
    [OpCodeCharacteristics("f64x2.max")]
    Float64x2Max = 0xF5,
    /// <summary>f64x2 pseudo-min.</summary>
    [OpCodeCharacteristics("f64x2.pmin")]
    Float64x2Pmin = 0xF6,
    /// <summary>f64x2 pseudo-max.</summary>
    [OpCodeCharacteristics("f64x2.pmax")]
    Float64x2Pmax = 0xF7,

    // ---- integer/float conversions ----
    /// <summary>Truncate f32x4 to i32x4, signed, with saturation.</summary>
    [OpCodeCharacteristics("i32x4.trunc_sat_f32x4_s")]
    Int32x4TruncSatFloat32x4Signed = 0xF8,
    /// <summary>Truncate f32x4 to i32x4, unsigned, with saturation.</summary>
    [OpCodeCharacteristics("i32x4.trunc_sat_f32x4_u")]
    Int32x4TruncSatFloat32x4Unsigned = 0xF9,
    /// <summary>Convert i32x4 to f32x4, signed.</summary>
    [OpCodeCharacteristics("f32x4.convert_i32x4_s")]
    Float32x4ConvertInt32x4Signed = 0xFA,
    /// <summary>Convert i32x4 to f32x4, unsigned.</summary>
    [OpCodeCharacteristics("f32x4.convert_i32x4_u")]
    Float32x4ConvertInt32x4Unsigned = 0xFB,
    /// <summary>Truncate f64x2 to i32x4, signed, with saturation (zero-extend).</summary>
    [OpCodeCharacteristics("i32x4.trunc_sat_f64x2_s_zero")]
    Int32x4TruncSatFloat64x2SignedZero = 0xFC,
    /// <summary>Truncate f64x2 to i32x4, unsigned, with saturation (zero-extend).</summary>
    [OpCodeCharacteristics("i32x4.trunc_sat_f64x2_u_zero")]
    Int32x4TruncSatFloat64x2UnsignedZero = 0xFD,
    /// <summary>Convert signed i32x4 (low) to f64x2.</summary>
    [OpCodeCharacteristics("f64x2.convert_low_i32x4_s")]
    Float64x2ConvertLowInt32x4Signed = 0xFE,
    /// <summary>Convert unsigned i32x4 (low) to f64x2.</summary>
    [OpCodeCharacteristics("f64x2.convert_low_i32x4_u")]
    Float64x2ConvertLowInt32x4Unsigned = 0xFF,
}

static class SimdOpCodeExtensions
{
    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, string>> opCodeNativeNamesByOpCode = new(
        () => typeof(SimdOpCode)
            .GetFields()
            .Where(field => field.IsStatic)
            .Select(field => new KeyValuePair<SimdOpCode, string>(
                (SimdOpCode)field.GetValue(null)!,
                field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name))
            .ToDictionary(kv => kv.Key, kv => kv.Value)
    );

    public static string ToNativeName(this SimdOpCode opCode)
    {
        opCodeNativeNamesByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }
}

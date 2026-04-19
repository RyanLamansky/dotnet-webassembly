using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace WebAssembly.Runtime;

/// <summary>
/// Runtime helpers for SIMD v128 load/store/const operations.
/// On .NET 5+ these operate on <c>Vector128&lt;byte&gt;</c>;
/// on older runtimes they use <see cref="V128Polyfill"/>.
/// </summary>
public static class V128Helper
{
    internal static readonly RegeneratingWeakReference<MethodInfo> ReadUnalignedMethod = new(()
        => typeof(V128Helper).GetMethod(nameof(ReadUnaligned), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> WriteUnalignedMethod = new(()
        => typeof(V128Helper).GetMethod(nameof(WriteUnaligned), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CreateMethod = new(()
        => typeof(V128Helper).GetMethod(nameof(Create), BindingFlags.Public | BindingFlags.Static)!);

    // --- v128 bitwise ---
    internal static readonly RegeneratingWeakReference<MethodInfo> V128NotMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Not), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128AndMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128And), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128AndNotMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128AndNot), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128OrMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Or), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128XorMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Xor), BindingFlags.Public | BindingFlags.Static)!);

    // --- i8x16 ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16AbsMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Abs), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16NegMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Neg), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16AddMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Add), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16SubMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Sub), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16AddSatSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16AddSatS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16AddSatUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16AddSatU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16SubSatSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16SubSatS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16SubSatUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16SubSatU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16MinSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16MinS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16MinUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16MinU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16MaxSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16MaxS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16MaxUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16MaxU), BindingFlags.Public | BindingFlags.Static)!);

    // --- i16x8 ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8AbsMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Abs), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8NegMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Neg), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8AddMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Add), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8SubMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Sub), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8MulMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Mul), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8AddSatSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8AddSatS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8AddSatUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8AddSatU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8SubSatSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8SubSatS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8SubSatUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8SubSatU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8MinSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8MinS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8MinUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8MinU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8MaxSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8MaxS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8MaxUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8MaxU), BindingFlags.Public | BindingFlags.Static)!);

    // --- i32x4 ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4AbsMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Abs), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4NegMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Neg), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4AddMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Add), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4SubMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Sub), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4MulMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Mul), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4MinSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4MinS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4MinUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4MinU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4MaxSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4MaxS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4MaxUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4MaxU), BindingFlags.Public | BindingFlags.Static)!);

    // --- i64x2 ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2AbsMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Abs), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2NegMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Neg), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2AddMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Add), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2SubMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Sub), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2MulMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Mul), BindingFlags.Public | BindingFlags.Static)!);

    // --- f32x4 ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4AbsMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Abs), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4NegMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Neg), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4SqrtMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Sqrt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4CeilMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Ceil), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4FloorMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Floor), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4TruncMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Trunc), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4NearestMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Nearest), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4AddMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Add), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4SubMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Sub), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4MulMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Mul), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4DivMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Div), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4MinMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Min), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4MaxMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Max), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4PminMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Pmin), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4PmaxMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Pmax), BindingFlags.Public | BindingFlags.Static)!);

    // --- f64x2 ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2AbsMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Abs), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2NegMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Neg), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2SqrtMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Sqrt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2CeilMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Ceil), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2FloorMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Floor), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2TruncMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Trunc), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2NearestMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Nearest), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2AddMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Add), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2SubMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Sub), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2MulMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Mul), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2DivMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Div), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2MinMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Min), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2MaxMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Max), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2PminMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Pmin), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2PmaxMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Pmax), BindingFlags.Public | BindingFlags.Static)!);

    // --- shuffle / swizzle ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ShuffleMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Shuffle), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16SwizzleMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Swizzle), BindingFlags.Public | BindingFlags.Static)!);

    // --- splats ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Splat), BindingFlags.Public | BindingFlags.Static)!);

    // --- extract lane ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ExtractLaneSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16ExtractLaneS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ExtractLaneUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16ExtractLaneU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtractLaneSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtractLaneS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtractLaneUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtractLaneU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtractLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtractLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtractLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtractLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4ExtractLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4ExtractLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2ExtractLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2ExtractLane), BindingFlags.Public | BindingFlags.Static)!);

    // --- replace lane ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ReplaceLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16ReplaceLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ReplaceLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ReplaceLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ReplaceLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ReplaceLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ReplaceLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ReplaceLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4ReplaceLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4ReplaceLane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2ReplaceLaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2ReplaceLane), BindingFlags.Public | BindingFlags.Static)!);

#if NET5_0_OR_GREATER
    /// <summary>The CLR type used to represent v128 at runtime on this platform.</summary>
    public static Type V128Type => typeof(System.Runtime.Intrinsics.Vector128<byte>);

    /// <summary>Read a 128-bit vector from an unaligned native pointer.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector128<byte> ReadUnaligned(IntPtr ptr)
        => Unsafe.ReadUnaligned<System.Runtime.Intrinsics.Vector128<byte>>((void*)ptr);

    /// <summary>Write a 128-bit vector to an unaligned native pointer.</summary>
    public static unsafe void WriteUnaligned(IntPtr ptr, System.Runtime.Intrinsics.Vector128<byte> value)
        => Unsafe.WriteUnaligned<System.Runtime.Intrinsics.Vector128<byte>>((void*)ptr, value);

    /// <summary>Create a v128 from 16 bytes.</summary>
    public static Vector128<byte> Create(
        byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7,
        byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14, byte b15)
        => Vector128.Create(b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15);

    /// <summary>v128 bitwise NOT.</summary>
    public static Vector128<byte> V128Not(Vector128<byte> a) => ~a;
    /// <summary>v128 bitwise AND.</summary>
    public static Vector128<byte> V128And(Vector128<byte> a, Vector128<byte> b) => a & b;
    /// <summary>v128 bitwise ANDNOT (a &amp; ~b).</summary>
    public static Vector128<byte> V128AndNot(Vector128<byte> a, Vector128<byte> b) => Vector128.AndNot(a, b);
    /// <summary>v128 bitwise OR.</summary>
    public static Vector128<byte> V128Or(Vector128<byte> a, Vector128<byte> b) => a | b;
    /// <summary>v128 bitwise XOR.</summary>
    public static Vector128<byte> V128Xor(Vector128<byte> a, Vector128<byte> b) => a ^ b;

    /// <summary>i8x16 shuffle (two vectors, 16 byte lane indices 0-31).</summary>
    public static Vector128<byte> Int8x16Shuffle(Vector128<byte> a, Vector128<byte> b, byte[] indices)
    {
        var src = new byte[32];
        for (var i = 0; i < 16; i++) { src[i] = a.GetElement(i); src[16+i] = b.GetElement(i); }
        var r = new byte[16];
        for (var i = 0; i < 16; i++) r[i] = indices[i] < 32 ? src[indices[i]] : (byte)0;
        return Vector128.Create(r);
    }
    /// <summary>i8x16 swizzle (select lanes of a by indices in b, 0-15; out-of-range → 0).</summary>
    public static Vector128<byte> Int8x16Swizzle(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new byte[16];
        for (var i = 0; i < 16; i++) { var idx = b.GetElement(i); r[i] = idx < 16 ? a.GetElement(idx) : (byte)0; }
        return Vector128.Create(r);
    }

    /// <summary>Splat i32 to all i8x16 lanes.</summary>
    public static Vector128<byte> Int8x16Splat(int x) => Vector128.Create((byte)(x & 0xFF));
    /// <summary>Splat i32 to all i16x8 lanes.</summary>
    public static Vector128<byte> Int16x8Splat(int x) => Vector128.Create((short)(x & 0xFFFF)).AsByte();
    /// <summary>Splat i32 to all i32x4 lanes.</summary>
    public static Vector128<byte> Int32x4Splat(int x) => Vector128.Create(x).AsByte();
    /// <summary>Splat i64 to all i64x2 lanes.</summary>
    public static Vector128<byte> Int64x2Splat(long x) => Vector128.Create(x).AsByte();
    /// <summary>Splat f32 to all f32x4 lanes.</summary>
    public static Vector128<byte> Float32x4Splat(float x) => Vector128.Create(x).AsByte();
    /// <summary>Splat f64 to both f64x2 lanes.</summary>
    public static Vector128<byte> Float64x2Splat(double x) => Vector128.Create(x).AsByte();

    /// <summary>Extract signed i8 lane as i32.</summary>
    public static int Int8x16ExtractLaneS(Vector128<byte> v, int lane) => (sbyte)v.GetElement(lane);
    /// <summary>Extract unsigned i8 lane as i32.</summary>
    public static int Int8x16ExtractLaneU(Vector128<byte> v, int lane) => v.GetElement(lane);
    /// <summary>Extract signed i16 lane as i32.</summary>
    public static int Int16x8ExtractLaneS(Vector128<byte> v, int lane) => v.AsInt16().GetElement(lane);
    /// <summary>Extract unsigned i16 lane as i32.</summary>
    public static int Int16x8ExtractLaneU(Vector128<byte> v, int lane) => v.AsUInt16().GetElement(lane);
    /// <summary>Extract i32 lane.</summary>
    public static int Int32x4ExtractLane(Vector128<byte> v, int lane) => v.AsInt32().GetElement(lane);
    /// <summary>Extract i64 lane.</summary>
    public static long Int64x2ExtractLane(Vector128<byte> v, int lane) => v.AsInt64().GetElement(lane);
    /// <summary>Extract f32 lane.</summary>
    public static float Float32x4ExtractLane(Vector128<byte> v, int lane) => v.AsSingle().GetElement(lane);
    /// <summary>Extract f64 lane.</summary>
    public static double Float64x2ExtractLane(Vector128<byte> v, int lane) => v.AsDouble().GetElement(lane);

    /// <summary>Replace i8x16 lane with low byte of i32.</summary>
    public static Vector128<byte> Int8x16ReplaceLane(Vector128<byte> v, int lane, int x) => v.WithElement(lane, (byte)(x & 0xFF));
    /// <summary>Replace i16x8 lane with low 16 bits of i32.</summary>
    public static Vector128<byte> Int16x8ReplaceLane(Vector128<byte> v, int lane, int x) => v.AsInt16().WithElement(lane, (short)(x & 0xFFFF)).AsByte();
    /// <summary>Replace i32x4 lane.</summary>
    public static Vector128<byte> Int32x4ReplaceLane(Vector128<byte> v, int lane, int x) => v.AsInt32().WithElement(lane, x).AsByte();
    /// <summary>Replace i64x2 lane.</summary>
    public static Vector128<byte> Int64x2ReplaceLane(Vector128<byte> v, int lane, long x) => v.AsInt64().WithElement(lane, x).AsByte();
    /// <summary>Replace f32x4 lane.</summary>
    public static Vector128<byte> Float32x4ReplaceLane(Vector128<byte> v, int lane, float x) => v.AsSingle().WithElement(lane, x).AsByte();
    /// <summary>Replace f64x2 lane.</summary>
    public static Vector128<byte> Float64x2ReplaceLane(Vector128<byte> v, int lane, double x) => v.AsDouble().WithElement(lane, x).AsByte();

    /// <summary>i8x16 absolute value.</summary>
    public static Vector128<byte> Int8x16Abs(Vector128<byte> a) => Vector128.Abs(a.AsSByte()).AsByte();
    /// <summary>i8x16 negate.</summary>
    public static Vector128<byte> Int8x16Neg(Vector128<byte> a) => (-a.AsSByte()).AsByte();
    /// <summary>i8x16 add.</summary>
    public static Vector128<byte> Int8x16Add(Vector128<byte> a, Vector128<byte> b) => (a.AsSByte() + b.AsSByte()).AsByte();
    /// <summary>i8x16 subtract.</summary>
    public static Vector128<byte> Int8x16Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsSByte() - b.AsSByte()).AsByte();
    /// <summary>i8x16 signed saturating add.</summary>
    public static Vector128<byte> Int8x16AddSatS(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new sbyte[16];
        for (var i = 0; i < 16; i++) { var v = a.AsSByte().GetElement(i) + b.AsSByte().GetElement(i); r[i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>i8x16 unsigned saturating add.</summary>
    public static Vector128<byte> Int8x16AddSatU(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new byte[16];
        for (var i = 0; i < 16; i++) { var v = a.GetElement(i) + b.GetElement(i); r[i] = v > 255 ? (byte)255 : (byte)v; }
        return Vector128.Create(r);
    }
    /// <summary>i8x16 signed saturating subtract.</summary>
    public static Vector128<byte> Int8x16SubSatS(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new sbyte[16];
        for (var i = 0; i < 16; i++) { var v = a.AsSByte().GetElement(i) - b.AsSByte().GetElement(i); r[i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>i8x16 unsigned saturating subtract.</summary>
    public static Vector128<byte> Int8x16SubSatU(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new byte[16];
        for (var i = 0; i < 16; i++) { var x = a.GetElement(i); var y = b.GetElement(i); r[i] = x < y ? (byte)0 : (byte)(x - y); }
        return Vector128.Create(r);
    }
    /// <summary>i8x16 signed min.</summary>
    public static Vector128<byte> Int8x16MinS(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsSByte(), b.AsSByte()).AsByte();
    /// <summary>i8x16 unsigned min.</summary>
    public static Vector128<byte> Int8x16MinU(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a, b);
    /// <summary>i8x16 signed max.</summary>
    public static Vector128<byte> Int8x16MaxS(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsSByte(), b.AsSByte()).AsByte();
    /// <summary>i8x16 unsigned max.</summary>
    public static Vector128<byte> Int8x16MaxU(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a, b);

    /// <summary>i16x8 absolute value.</summary>
    public static Vector128<byte> Int16x8Abs(Vector128<byte> a) => Vector128.Abs(a.AsInt16()).AsByte();
    /// <summary>i16x8 negate.</summary>
    public static Vector128<byte> Int16x8Neg(Vector128<byte> a) => (-a.AsInt16()).AsByte();
    /// <summary>i16x8 add.</summary>
    public static Vector128<byte> Int16x8Add(Vector128<byte> a, Vector128<byte> b) => (a.AsInt16() + b.AsInt16()).AsByte();
    /// <summary>i16x8 subtract.</summary>
    public static Vector128<byte> Int16x8Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsInt16() - b.AsInt16()).AsByte();
    /// <summary>i16x8 multiply.</summary>
    public static Vector128<byte> Int16x8Mul(Vector128<byte> a, Vector128<byte> b) => (a.AsInt16() * b.AsInt16()).AsByte();
    /// <summary>i16x8 signed saturating add.</summary>
    public static Vector128<byte> Int16x8AddSatS(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new short[8];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i) + b.AsInt16().GetElement(i); r[i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>i16x8 unsigned saturating add.</summary>
    public static Vector128<byte> Int16x8AddSatU(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new ushort[8];
        for (var i = 0; i < 8; i++) { var v = a.AsUInt16().GetElement(i) + b.AsUInt16().GetElement(i); r[i] = v > 65535u ? (ushort)65535 : (ushort)v; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>i16x8 signed saturating subtract.</summary>
    public static Vector128<byte> Int16x8SubSatS(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new short[8];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i) - b.AsInt16().GetElement(i); r[i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>i16x8 unsigned saturating subtract.</summary>
    public static Vector128<byte> Int16x8SubSatU(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new ushort[8];
        for (var i = 0; i < 8; i++) { var x = a.AsUInt16().GetElement(i); var y = b.AsUInt16().GetElement(i); r[i] = x < y ? (ushort)0 : (ushort)(x - y); }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>i16x8 signed min.</summary>
    public static Vector128<byte> Int16x8MinS(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsInt16(), b.AsInt16()).AsByte();
    /// <summary>i16x8 unsigned min.</summary>
    public static Vector128<byte> Int16x8MinU(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsUInt16(), b.AsUInt16()).AsByte();
    /// <summary>i16x8 signed max.</summary>
    public static Vector128<byte> Int16x8MaxS(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsInt16(), b.AsInt16()).AsByte();
    /// <summary>i16x8 unsigned max.</summary>
    public static Vector128<byte> Int16x8MaxU(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsUInt16(), b.AsUInt16()).AsByte();

    /// <summary>i32x4 absolute value.</summary>
    public static Vector128<byte> Int32x4Abs(Vector128<byte> a) => Vector128.Abs(a.AsInt32()).AsByte();
    /// <summary>i32x4 negate.</summary>
    public static Vector128<byte> Int32x4Neg(Vector128<byte> a) => (-a.AsInt32()).AsByte();
    /// <summary>i32x4 add.</summary>
    public static Vector128<byte> Int32x4Add(Vector128<byte> a, Vector128<byte> b) => (a.AsInt32() + b.AsInt32()).AsByte();
    /// <summary>i32x4 subtract.</summary>
    public static Vector128<byte> Int32x4Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsInt32() - b.AsInt32()).AsByte();
    /// <summary>i32x4 multiply.</summary>
    public static Vector128<byte> Int32x4Mul(Vector128<byte> a, Vector128<byte> b) => (a.AsInt32() * b.AsInt32()).AsByte();
    /// <summary>i32x4 signed min.</summary>
    public static Vector128<byte> Int32x4MinS(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsInt32(), b.AsInt32()).AsByte();
    /// <summary>i32x4 unsigned min.</summary>
    public static Vector128<byte> Int32x4MinU(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsUInt32(), b.AsUInt32()).AsByte();
    /// <summary>i32x4 signed max.</summary>
    public static Vector128<byte> Int32x4MaxS(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsInt32(), b.AsInt32()).AsByte();
    /// <summary>i32x4 unsigned max.</summary>
    public static Vector128<byte> Int32x4MaxU(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsUInt32(), b.AsUInt32()).AsByte();

    /// <summary>i64x2 absolute value.</summary>
    public static Vector128<byte> Int64x2Abs(Vector128<byte> a) => Vector128.Abs(a.AsInt64()).AsByte();
    /// <summary>i64x2 negate.</summary>
    public static Vector128<byte> Int64x2Neg(Vector128<byte> a) => (-a.AsInt64()).AsByte();
    /// <summary>i64x2 add.</summary>
    public static Vector128<byte> Int64x2Add(Vector128<byte> a, Vector128<byte> b) => (a.AsInt64() + b.AsInt64()).AsByte();
    /// <summary>i64x2 subtract.</summary>
    public static Vector128<byte> Int64x2Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsInt64() - b.AsInt64()).AsByte();
    /// <summary>i64x2 multiply.</summary>
    public static Vector128<byte> Int64x2Mul(Vector128<byte> a, Vector128<byte> b) => (a.AsInt64() * b.AsInt64()).AsByte();

    /// <summary>f32x4 absolute value.</summary>
    public static Vector128<byte> Float32x4Abs(Vector128<byte> a) => Vector128.Abs(a.AsSingle()).AsByte();
    /// <summary>f32x4 negate.</summary>
    public static Vector128<byte> Float32x4Neg(Vector128<byte> a) => (-a.AsSingle()).AsByte();
    /// <summary>f32x4 square root.</summary>
    public static Vector128<byte> Float32x4Sqrt(Vector128<byte> a) => Vector128.Sqrt(a.AsSingle()).AsByte();
    /// <summary>f32x4 ceiling.</summary>
    public static Vector128<byte> Float32x4Ceil(Vector128<byte> a) => Vector128.Ceiling(a.AsSingle()).AsByte();
    /// <summary>f32x4 floor.</summary>
    public static Vector128<byte> Float32x4Floor(Vector128<byte> a) => Vector128.Floor(a.AsSingle()).AsByte();
    /// <summary>f32x4 truncate toward zero.</summary>
    public static Vector128<byte> Float32x4Trunc(Vector128<byte> a)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) r[i] = MathF.Truncate(a.AsSingle().GetElement(i));
        return Vector128.Create(r).AsByte();
    }
    /// <summary>f32x4 round to nearest even.</summary>
    public static Vector128<byte> Float32x4Nearest(Vector128<byte> a)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) r[i] = MathF.Round(a.AsSingle().GetElement(i), MidpointRounding.ToEven);
        return Vector128.Create(r).AsByte();
    }
    /// <summary>f32x4 add.</summary>
    public static Vector128<byte> Float32x4Add(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() + b.AsSingle()).AsByte();
    /// <summary>f32x4 subtract.</summary>
    public static Vector128<byte> Float32x4Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() - b.AsSingle()).AsByte();
    /// <summary>f32x4 multiply.</summary>
    public static Vector128<byte> Float32x4Mul(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() * b.AsSingle()).AsByte();
    /// <summary>f32x4 divide.</summary>
    public static Vector128<byte> Float32x4Div(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() / b.AsSingle()).AsByte();
    /// <summary>f32x4 IEEE min (propagates NaN).</summary>
    public static Vector128<byte> Float32x4Min(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsSingle(), b.AsSingle()).AsByte();
    /// <summary>f32x4 IEEE max (propagates NaN).</summary>
    public static Vector128<byte> Float32x4Max(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsSingle(), b.AsSingle()).AsByte();
    /// <summary>f32x4 pseudo-min (returns b if b &lt; a, else a).</summary>
    public static Vector128<byte> Float32x4Pmin(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) { var ai = a.AsSingle().GetElement(i); var bi = b.AsSingle().GetElement(i); r[i] = bi < ai ? bi : ai; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>f32x4 pseudo-max (returns b if b &gt; a, else a).</summary>
    public static Vector128<byte> Float32x4Pmax(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) { var ai = a.AsSingle().GetElement(i); var bi = b.AsSingle().GetElement(i); r[i] = bi > ai ? bi : ai; }
        return Vector128.Create(r).AsByte();
    }

    /// <summary>f64x2 absolute value.</summary>
    public static Vector128<byte> Float64x2Abs(Vector128<byte> a) => Vector128.Abs(a.AsDouble()).AsByte();
    /// <summary>f64x2 negate.</summary>
    public static Vector128<byte> Float64x2Neg(Vector128<byte> a) => (-a.AsDouble()).AsByte();
    /// <summary>f64x2 square root.</summary>
    public static Vector128<byte> Float64x2Sqrt(Vector128<byte> a) => Vector128.Sqrt(a.AsDouble()).AsByte();
    /// <summary>f64x2 ceiling.</summary>
    public static Vector128<byte> Float64x2Ceil(Vector128<byte> a) => Vector128.Ceiling(a.AsDouble()).AsByte();
    /// <summary>f64x2 floor.</summary>
    public static Vector128<byte> Float64x2Floor(Vector128<byte> a) => Vector128.Floor(a.AsDouble()).AsByte();
    /// <summary>f64x2 truncate toward zero.</summary>
    public static Vector128<byte> Float64x2Trunc(Vector128<byte> a)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) r[i] = Math.Truncate(a.AsDouble().GetElement(i));
        return Vector128.Create(r).AsByte();
    }
    /// <summary>f64x2 round to nearest even.</summary>
    public static Vector128<byte> Float64x2Nearest(Vector128<byte> a)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) r[i] = Math.Round(a.AsDouble().GetElement(i), MidpointRounding.ToEven);
        return Vector128.Create(r).AsByte();
    }
    /// <summary>f64x2 add.</summary>
    public static Vector128<byte> Float64x2Add(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() + b.AsDouble()).AsByte();
    /// <summary>f64x2 subtract.</summary>
    public static Vector128<byte> Float64x2Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() - b.AsDouble()).AsByte();
    /// <summary>f64x2 multiply.</summary>
    public static Vector128<byte> Float64x2Mul(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() * b.AsDouble()).AsByte();
    /// <summary>f64x2 divide.</summary>
    public static Vector128<byte> Float64x2Div(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() / b.AsDouble()).AsByte();
    /// <summary>f64x2 IEEE min (propagates NaN).</summary>
    public static Vector128<byte> Float64x2Min(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsDouble(), b.AsDouble()).AsByte();
    /// <summary>f64x2 IEEE max (propagates NaN).</summary>
    public static Vector128<byte> Float64x2Max(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsDouble(), b.AsDouble()).AsByte();
    /// <summary>f64x2 pseudo-min (returns b if b &lt; a, else a).</summary>
    public static Vector128<byte> Float64x2Pmin(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) { var ai = a.AsDouble().GetElement(i); var bi = b.AsDouble().GetElement(i); r[i] = bi < ai ? bi : ai; }
        return Vector128.Create(r).AsByte();
    }
    /// <summary>f64x2 pseudo-max (returns b if b &gt; a, else a).</summary>
    public static Vector128<byte> Float64x2Pmax(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) { var ai = a.AsDouble().GetElement(i); var bi = b.AsDouble().GetElement(i); r[i] = bi > ai ? bi : ai; }
        return Vector128.Create(r).AsByte();
    }
#else
    /// <summary>The CLR type used to represent v128 at runtime on this platform.</summary>
    public static Type V128Type => typeof(V128Polyfill);

    /// <summary>Read a 128-bit vector from an unaligned native pointer.</summary>
    public static unsafe V128Polyfill ReadUnaligned(IntPtr ptr)
        => Unsafe.ReadUnaligned<V128Polyfill>((void*)ptr);

    /// <summary>Write a 128-bit vector to an unaligned native pointer.</summary>
    public static unsafe void WriteUnaligned(IntPtr ptr, V128Polyfill value)
        => Unsafe.WriteUnaligned<V128Polyfill>((void*)ptr, value);

    /// <summary>Create a v128 from 16 bytes.</summary>
    public static V128Polyfill Create(
        byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7,
        byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14, byte b15)
        => new()
        {
            B0 = b0, B1 = b1, B2 = b2, B3 = b3, B4 = b4, B5 = b5, B6 = b6, B7 = b7,
            B8 = b8, B9 = b9, B10 = b10, B11 = b11, B12 = b12, B13 = b13, B14 = b14, B15 = b15,
        };

    private static V128Polyfill ApplyBinary(V128Polyfill a, V128Polyfill b, Func<byte, byte, byte> op)
        => new() {
            B0 = op(a.B0, b.B0), B1 = op(a.B1, b.B1), B2 = op(a.B2, b.B2), B3 = op(a.B3, b.B3),
            B4 = op(a.B4, b.B4), B5 = op(a.B5, b.B5), B6 = op(a.B6, b.B6), B7 = op(a.B7, b.B7),
            B8 = op(a.B8, b.B8), B9 = op(a.B9, b.B9), B10 = op(a.B10, b.B10), B11 = op(a.B11, b.B11),
            B12 = op(a.B12, b.B12), B13 = op(a.B13, b.B13), B14 = op(a.B14, b.B14), B15 = op(a.B15, b.B15),
        };

    private static V128Polyfill ApplyUnary(V128Polyfill a, Func<byte, byte> op)
        => new() {
            B0 = op(a.B0), B1 = op(a.B1), B2 = op(a.B2), B3 = op(a.B3),
            B4 = op(a.B4), B5 = op(a.B5), B6 = op(a.B6), B7 = op(a.B7),
            B8 = op(a.B8), B9 = op(a.B9), B10 = op(a.B10), B11 = op(a.B11),
            B12 = op(a.B12), B13 = op(a.B13), B14 = op(a.B14), B15 = op(a.B15),
        };

    private static unsafe (byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7,
                            byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14, byte b15)
        GetBytes(V128Polyfill v) => (v.B0, v.B1, v.B2, v.B3, v.B4, v.B5, v.B6, v.B7,
                                     v.B8, v.B9, v.B10, v.B11, v.B12, v.B13, v.B14, v.B15);

#pragma warning disable CS1591
    // shuffle / swizzle
    public static V128Polyfill Int8x16Shuffle(V128Polyfill a, V128Polyfill b, byte[] indices)
    {
        var src = new byte[] { a.B0,a.B1,a.B2,a.B3,a.B4,a.B5,a.B6,a.B7,a.B8,a.B9,a.B10,a.B11,a.B12,a.B13,a.B14,a.B15,
                               b.B0,b.B1,b.B2,b.B3,b.B4,b.B5,b.B6,b.B7,b.B8,b.B9,b.B10,b.B11,b.B12,b.B13,b.B14,b.B15 };
        var r = new byte[16];
        for (var i = 0; i < 16; i++) r[i] = indices[i] < 32 ? src[indices[i]] : (byte)0;
        return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]);
    }
    public static V128Polyfill Int8x16Swizzle(V128Polyfill a, V128Polyfill b)
    {
        var src = new byte[] { a.B0,a.B1,a.B2,a.B3,a.B4,a.B5,a.B6,a.B7,a.B8,a.B9,a.B10,a.B11,a.B12,a.B13,a.B14,a.B15 };
        var idx = new byte[] { b.B0,b.B1,b.B2,b.B3,b.B4,b.B5,b.B6,b.B7,b.B8,b.B9,b.B10,b.B11,b.B12,b.B13,b.B14,b.B15 };
        var r = new byte[16];
        for (var i = 0; i < 16; i++) r[i] = idx[i] < 16 ? src[idx[i]] : (byte)0;
        return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]);
    }
    // splats
    public static V128Polyfill Int8x16Splat(int x) { var b = (byte)(x & 0xFF); return Create(b,b,b,b,b,b,b,b,b,b,b,b,b,b,b,b); }
    public static V128Polyfill Int16x8Splat(int x)
    {
        var lo = (byte)(x & 0xFF); var hi = (byte)((x >> 8) & 0xFF);
        return Create(lo,hi,lo,hi,lo,hi,lo,hi,lo,hi,lo,hi,lo,hi,lo,hi);
    }
    public static V128Polyfill Int32x4Splat(int x)
    {
        var b0=(byte)x; var b1=(byte)(x>>8); var b2=(byte)(x>>16); var b3=(byte)(x>>24);
        return Create(b0,b1,b2,b3,b0,b1,b2,b3,b0,b1,b2,b3,b0,b1,b2,b3);
    }
    public static V128Polyfill Int64x2Splat(long x)
    {
        var b0=(byte)x;var b1=(byte)(x>>8);var b2=(byte)(x>>16);var b3=(byte)(x>>24);
        var b4=(byte)(x>>32);var b5=(byte)(x>>40);var b6=(byte)(x>>48);var b7=(byte)(x>>56);
        return Create(b0,b1,b2,b3,b4,b5,b6,b7,b0,b1,b2,b3,b4,b5,b6,b7);
    }
    public static V128Polyfill Float32x4Splat(float x)
    {
        var u = Unsafe.As<float, uint>(ref x);
        var b0=(byte)u;var b1=(byte)(u>>8);var b2=(byte)(u>>16);var b3=(byte)(u>>24);
        return Create(b0,b1,b2,b3,b0,b1,b2,b3,b0,b1,b2,b3,b0,b1,b2,b3);
    }
    public static V128Polyfill Float64x2Splat(double x)
    {
        var u = Unsafe.As<double, ulong>(ref x);
        var b0=(byte)u;var b1=(byte)(u>>8);var b2=(byte)(u>>16);var b3=(byte)(u>>24);
        var b4=(byte)(u>>32);var b5=(byte)(u>>40);var b6=(byte)(u>>48);var b7=(byte)(u>>56);
        return Create(b0,b1,b2,b3,b4,b5,b6,b7,b0,b1,b2,b3,b4,b5,b6,b7);
    }
    // extract lane
    private static byte GetByte(V128Polyfill v, int i) => i switch { 0=>v.B0,1=>v.B1,2=>v.B2,3=>v.B3,4=>v.B4,5=>v.B5,6=>v.B6,7=>v.B7,8=>v.B8,9=>v.B9,10=>v.B10,11=>v.B11,12=>v.B12,13=>v.B13,14=>v.B14,_=>v.B15 };
    public static int Int8x16ExtractLaneS(V128Polyfill v, int lane) => (sbyte)GetByte(v, lane);
    public static int Int8x16ExtractLaneU(V128Polyfill v, int lane) => GetByte(v, lane);
    public static int Int16x8ExtractLaneS(V128Polyfill v, int lane) { var b = lane*2; return (short)(GetByte(v,b)|(GetByte(v,b+1)<<8)); }
    public static int Int16x8ExtractLaneU(V128Polyfill v, int lane) { var b = lane*2; return (ushort)(GetByte(v,b)|(GetByte(v,b+1)<<8)); }
    public static int Int32x4ExtractLane(V128Polyfill v, int lane) { var b = lane*4; return GetByte(v,b)|(GetByte(v,b+1)<<8)|(GetByte(v,b+2)<<16)|(GetByte(v,b+3)<<24); }
    public static long Int64x2ExtractLane(V128Polyfill v, int lane) { var b = lane*8; return (long)((ulong)GetByte(v,b)|((ulong)GetByte(v,b+1)<<8)|((ulong)GetByte(v,b+2)<<16)|((ulong)GetByte(v,b+3)<<24)|((ulong)GetByte(v,b+4)<<32)|((ulong)GetByte(v,b+5)<<40)|((ulong)GetByte(v,b+6)<<48)|((ulong)GetByte(v,b+7)<<56)); }
    public static float Float32x4ExtractLane(V128Polyfill v, int lane) => GetF32(v, lane*4);
    public static double Float64x2ExtractLane(V128Polyfill v, int lane) => lane == 0 ? GetF64Lo(v) : GetF64Hi(v);
    // replace lane
    private static V128Polyfill SetByte(V128Polyfill v, int i, byte val) { switch(i){case 0:v.B0=val;break;case 1:v.B1=val;break;case 2:v.B2=val;break;case 3:v.B3=val;break;case 4:v.B4=val;break;case 5:v.B5=val;break;case 6:v.B6=val;break;case 7:v.B7=val;break;case 8:v.B8=val;break;case 9:v.B9=val;break;case 10:v.B10=val;break;case 11:v.B11=val;break;case 12:v.B12=val;break;case 13:v.B13=val;break;case 14:v.B14=val;break;default:v.B15=val;break;} return v; }
    public static V128Polyfill Int8x16ReplaceLane(V128Polyfill v, int lane, int x) => SetByte(v, lane, (byte)(x & 0xFF));
    public static V128Polyfill Int16x8ReplaceLane(V128Polyfill v, int lane, int x) { var b = lane*2; v = SetByte(v,b,(byte)(x&0xFF)); v = SetByte(v,b+1,(byte)((x>>8)&0xFF)); return v; }
    public static V128Polyfill Int32x4ReplaceLane(V128Polyfill v, int lane, int x) { var b = lane*4; v = SetByte(v,b,(byte)x); v = SetByte(v,b+1,(byte)(x>>8)); v = SetByte(v,b+2,(byte)(x>>16)); v = SetByte(v,b+3,(byte)(x>>24)); return v; }
    public static V128Polyfill Int64x2ReplaceLane(V128Polyfill v, int lane, long x) { var b = lane*8; for(var i=0;i<8;i++) v = SetByte(v,b+i,(byte)(x>>(i*8))); return v; }
    public static V128Polyfill Float32x4ReplaceLane(V128Polyfill v, int lane, float x) => SetF32(v, lane*4, x);
    public static V128Polyfill Float64x2ReplaceLane(V128Polyfill v, int lane, double x) => SetF64(v, lane != 0, x);
    // v128 bitwise
    public static V128Polyfill V128Not(V128Polyfill a) => ApplyUnary(a, b => (byte)~b);
    public static V128Polyfill V128And(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)(x & y));
    public static V128Polyfill V128AndNot(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)(x & ~y));
    public static V128Polyfill V128Or(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)(x | y));
    public static V128Polyfill V128Xor(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)(x ^ y));

    // i8x16
    public static V128Polyfill Int8x16Abs(V128Polyfill a) => ApplyUnary(a, b => (byte)Math.Abs((sbyte)b));
    public static V128Polyfill Int8x16Neg(V128Polyfill a) => ApplyUnary(a, b => (byte)(-(sbyte)b));
    public static V128Polyfill Int8x16Add(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)((sbyte)x + (sbyte)y));
    public static V128Polyfill Int8x16Sub(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)((sbyte)x - (sbyte)y));
    public static V128Polyfill Int8x16AddSatS(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => { var r = (int)(sbyte)x + (sbyte)y; return (byte)(sbyte)(r < -128 ? -128 : r > 127 ? 127 : r); });
    public static V128Polyfill Int8x16AddSatU(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => { var r = x + y; return (byte)(r > 255 ? 255 : r); });
    public static V128Polyfill Int8x16SubSatS(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => { var r = (int)(sbyte)x - (sbyte)y; return (byte)(sbyte)(r < -128 ? -128 : r > 127 ? 127 : r); });
    public static V128Polyfill Int8x16SubSatU(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)(x < y ? 0 : x - y));
    public static V128Polyfill Int8x16MinS(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)((sbyte)x < (sbyte)y ? (sbyte)x : (sbyte)y));
    public static V128Polyfill Int8x16MinU(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => x < y ? x : y);
    public static V128Polyfill Int8x16MaxS(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)((sbyte)x > (sbyte)y ? (sbyte)x : (sbyte)y));
    public static V128Polyfill Int8x16MaxU(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => x > y ? x : y);

    // i16x8 — work on pairs of bytes as little-endian int16
    private static V128Polyfill ApplyI16Binary(V128Polyfill a, V128Polyfill b, Func<short, short, short> op)
    {
        static short Get(byte lo, byte hi) => (short)(lo | (hi << 8));
        static (byte lo, byte hi) Put(short v) => ((byte)v, (byte)((ushort)v >> 8));
        var (lo0, hi0) = Put(op(Get(a.B0, a.B1), Get(b.B0, b.B1)));
        var (lo1, hi1) = Put(op(Get(a.B2, a.B3), Get(b.B2, b.B3)));
        var (lo2, hi2) = Put(op(Get(a.B4, a.B5), Get(b.B4, b.B5)));
        var (lo3, hi3) = Put(op(Get(a.B6, a.B7), Get(b.B6, b.B7)));
        var (lo4, hi4) = Put(op(Get(a.B8, a.B9), Get(b.B8, b.B9)));
        var (lo5, hi5) = Put(op(Get(a.B10, a.B11), Get(b.B10, b.B11)));
        var (lo6, hi6) = Put(op(Get(a.B12, a.B13), Get(b.B12, b.B13)));
        var (lo7, hi7) = Put(op(Get(a.B14, a.B15), Get(b.B14, b.B15)));
        return new() { B0=lo0,B1=hi0,B2=lo1,B3=hi1,B4=lo2,B5=hi2,B6=lo3,B7=hi3,B8=lo4,B9=hi4,B10=lo5,B11=hi5,B12=lo6,B13=hi6,B14=lo7,B15=hi7 };
    }
    private static V128Polyfill ApplyI16Unary(V128Polyfill a, Func<short, short> op)
    {
        static short Get(byte lo, byte hi) => (short)(lo | (hi << 8));
        static (byte lo, byte hi) Put(short v) => ((byte)v, (byte)((ushort)v >> 8));
        var (lo0, hi0) = Put(op(Get(a.B0, a.B1)));
        var (lo1, hi1) = Put(op(Get(a.B2, a.B3)));
        var (lo2, hi2) = Put(op(Get(a.B4, a.B5)));
        var (lo3, hi3) = Put(op(Get(a.B6, a.B7)));
        var (lo4, hi4) = Put(op(Get(a.B8, a.B9)));
        var (lo5, hi5) = Put(op(Get(a.B10, a.B11)));
        var (lo6, hi6) = Put(op(Get(a.B12, a.B13)));
        var (lo7, hi7) = Put(op(Get(a.B14, a.B15)));
        return new() { B0=lo0,B1=hi0,B2=lo1,B3=hi1,B4=lo2,B5=hi2,B6=lo3,B7=hi3,B8=lo4,B9=hi4,B10=lo5,B11=hi5,B12=lo6,B13=hi6,B14=lo7,B15=hi7 };
    }
    public static V128Polyfill Int16x8Abs(V128Polyfill a) => ApplyI16Unary(a, x => x < 0 ? (short)-x : x);
    public static V128Polyfill Int16x8Neg(V128Polyfill a) => ApplyI16Unary(a, x => (short)-x);
    public static V128Polyfill Int16x8Add(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => (short)(x + y));
    public static V128Polyfill Int16x8Sub(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => (short)(x - y));
    public static V128Polyfill Int16x8Mul(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => (short)(x * y));
    public static V128Polyfill Int16x8AddSatS(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => { var r = (int)x + y; return (short)(r < -32768 ? -32768 : r > 32767 ? 32767 : r); });
    public static V128Polyfill Int16x8AddSatU(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => { var r = (ushort)x + (ushort)y; return (short)(r > 65535 ? 65535 : r); });
    public static V128Polyfill Int16x8SubSatS(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => { var r = (int)x - y; return (short)(r < -32768 ? -32768 : r > 32767 ? 32767 : r); });
    public static V128Polyfill Int16x8SubSatU(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => { var ux = (ushort)x; var uy = (ushort)y; return (short)(ushort)(ux < uy ? 0 : ux - uy); });
    public static V128Polyfill Int16x8MinS(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => x < y ? x : y);
    public static V128Polyfill Int16x8MinU(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => (short)((ushort)x < (ushort)y ? (ushort)x : (ushort)y));
    public static V128Polyfill Int16x8MaxS(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => x > y ? x : y);
    public static V128Polyfill Int16x8MaxU(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => (short)((ushort)x > (ushort)y ? (ushort)x : (ushort)y));

    // i32x4
    private static V128Polyfill ApplyI32Binary(V128Polyfill a, V128Polyfill b, Func<int, int, int> op)
    {
        static int Get(byte b0, byte b1, byte b2, byte b3) => b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
        static (byte b0, byte b1, byte b2, byte b3) Put(int v) => ((byte)v, (byte)(v >> 8), (byte)(v >> 16), (byte)(v >> 24));
        var (a0,a1,a2,a3) = Put(op(Get(a.B0,a.B1,a.B2,a.B3), Get(b.B0,b.B1,b.B2,b.B3)));
        var (a4,a5,a6,a7) = Put(op(Get(a.B4,a.B5,a.B6,a.B7), Get(b.B4,b.B5,b.B6,b.B7)));
        var (a8,a9,a10,a11) = Put(op(Get(a.B8,a.B9,a.B10,a.B11), Get(b.B8,b.B9,b.B10,b.B11)));
        var (a12,a13,a14,a15) = Put(op(Get(a.B12,a.B13,a.B14,a.B15), Get(b.B12,b.B13,b.B14,b.B15)));
        return new() { B0=a0,B1=a1,B2=a2,B3=a3,B4=a4,B5=a5,B6=a6,B7=a7,B8=a8,B9=a9,B10=a10,B11=a11,B12=a12,B13=a13,B14=a14,B15=a15 };
    }
    private static V128Polyfill ApplyI32Unary(V128Polyfill a, Func<int, int> op)
    {
        static int Get(byte b0, byte b1, byte b2, byte b3) => b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
        static (byte b0, byte b1, byte b2, byte b3) Put(int v) => ((byte)v, (byte)(v >> 8), (byte)(v >> 16), (byte)(v >> 24));
        var (a0,a1,a2,a3) = Put(op(Get(a.B0,a.B1,a.B2,a.B3)));
        var (a4,a5,a6,a7) = Put(op(Get(a.B4,a.B5,a.B6,a.B7)));
        var (a8,a9,a10,a11) = Put(op(Get(a.B8,a.B9,a.B10,a.B11)));
        var (a12,a13,a14,a15) = Put(op(Get(a.B12,a.B13,a.B14,a.B15)));
        return new() { B0=a0,B1=a1,B2=a2,B3=a3,B4=a4,B5=a5,B6=a6,B7=a7,B8=a8,B9=a9,B10=a10,B11=a11,B12=a12,B13=a13,B14=a14,B15=a15 };
    }
    public static V128Polyfill Int32x4Abs(V128Polyfill a) => ApplyI32Unary(a, x => x < 0 ? -x : x);
    public static V128Polyfill Int32x4Neg(V128Polyfill a) => ApplyI32Unary(a, x => -x);
    public static V128Polyfill Int32x4Add(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, (x, y) => x + y);
    public static V128Polyfill Int32x4Sub(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, (x, y) => x - y);
    public static V128Polyfill Int32x4Mul(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, (x, y) => x * y);
    public static V128Polyfill Int32x4MinS(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, Math.Min);
    public static V128Polyfill Int32x4MinU(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, (x, y) => (int)((uint)x < (uint)y ? (uint)x : (uint)y));
    public static V128Polyfill Int32x4MaxS(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, Math.Max);
    public static V128Polyfill Int32x4MaxU(V128Polyfill a, V128Polyfill b) => ApplyI32Binary(a, b, (x, y) => (int)((uint)x > (uint)y ? (uint)x : (uint)y));

    // i64x2
    private static V128Polyfill ApplyI64Binary(V128Polyfill a, V128Polyfill b, Func<long, long, long> op)
    {
        static long GetLo(V128Polyfill v) => (long)(v.B0 | ((ulong)v.B1 << 8) | ((ulong)v.B2 << 16) | ((ulong)v.B3 << 24) | ((ulong)v.B4 << 32) | ((ulong)v.B5 << 40) | ((ulong)v.B6 << 48) | ((ulong)v.B7 << 56));
        static long GetHi(V128Polyfill v) => (long)(v.B8 | ((ulong)v.B9 << 8) | ((ulong)v.B10 << 16) | ((ulong)v.B11 << 24) | ((ulong)v.B12 << 32) | ((ulong)v.B13 << 40) | ((ulong)v.B14 << 48) | ((ulong)v.B15 << 56));
        static (byte b0,byte b1,byte b2,byte b3,byte b4,byte b5,byte b6,byte b7) Put(long v) {
            var u = (ulong)v;
            return ((byte)u,(byte)(u>>8),(byte)(u>>16),(byte)(u>>24),(byte)(u>>32),(byte)(u>>40),(byte)(u>>48),(byte)(u>>56));
        }
        var (l0,l1,l2,l3,l4,l5,l6,l7) = Put(op(GetLo(a), GetLo(b)));
        var (h0,h1,h2,h3,h4,h5,h6,h7) = Put(op(GetHi(a), GetHi(b)));
        return new() { B0=l0,B1=l1,B2=l2,B3=l3,B4=l4,B5=l5,B6=l6,B7=l7,B8=h0,B9=h1,B10=h2,B11=h3,B12=h4,B13=h5,B14=h6,B15=h7 };
    }
    private static V128Polyfill ApplyI64Unary(V128Polyfill a, Func<long, long> op)
    {
        static long GetLo(V128Polyfill v) => (long)(v.B0 | ((ulong)v.B1 << 8) | ((ulong)v.B2 << 16) | ((ulong)v.B3 << 24) | ((ulong)v.B4 << 32) | ((ulong)v.B5 << 40) | ((ulong)v.B6 << 48) | ((ulong)v.B7 << 56));
        static long GetHi(V128Polyfill v) => (long)(v.B8 | ((ulong)v.B9 << 8) | ((ulong)v.B10 << 16) | ((ulong)v.B11 << 24) | ((ulong)v.B12 << 32) | ((ulong)v.B13 << 40) | ((ulong)v.B14 << 48) | ((ulong)v.B15 << 56));
        static (byte b0,byte b1,byte b2,byte b3,byte b4,byte b5,byte b6,byte b7) Put(long v) {
            var u = (ulong)v;
            return ((byte)u,(byte)(u>>8),(byte)(u>>16),(byte)(u>>24),(byte)(u>>32),(byte)(u>>40),(byte)(u>>48),(byte)(u>>56));
        }
        var (l0,l1,l2,l3,l4,l5,l6,l7) = Put(op(GetLo(a)));
        var (h0,h1,h2,h3,h4,h5,h6,h7) = Put(op(GetHi(a)));
        return new() { B0=l0,B1=l1,B2=l2,B3=l3,B4=l4,B5=l5,B6=l6,B7=l7,B8=h0,B9=h1,B10=h2,B11=h3,B12=h4,B13=h5,B14=h6,B15=h7 };
    }
    public static V128Polyfill Int64x2Abs(V128Polyfill a) => ApplyI64Unary(a, x => x < 0 ? -x : x);
    public static V128Polyfill Int64x2Neg(V128Polyfill a) => ApplyI64Unary(a, x => -x);
    public static V128Polyfill Int64x2Add(V128Polyfill a, V128Polyfill b) => ApplyI64Binary(a, b, (x, y) => x + y);
    public static V128Polyfill Int64x2Sub(V128Polyfill a, V128Polyfill b) => ApplyI64Binary(a, b, (x, y) => x - y);
    public static V128Polyfill Int64x2Mul(V128Polyfill a, V128Polyfill b) => ApplyI64Binary(a, b, (x, y) => x * y);

    // f32x4 helpers
    private static float GetF32(V128Polyfill v, int offset)
    {
        uint u = offset switch { 0 => (uint)(v.B0|(v.B1<<8)|(v.B2<<16)|(v.B3<<24)), 4 => (uint)(v.B4|(v.B5<<8)|(v.B6<<16)|(v.B7<<24)), 8 => (uint)(v.B8|(v.B9<<8)|(v.B10<<16)|(v.B11<<24)), _ => (uint)(v.B12|(v.B13<<8)|(v.B14<<16)|(v.B15<<24)) };
        return Unsafe.As<uint, float>(ref u);
    }
    private static V128Polyfill SetF32(V128Polyfill v, int offset, float f)
    {
        var u = Unsafe.As<float, uint>(ref f);
        var b0 = (byte)u; var b1 = (byte)(u>>8); var b2 = (byte)(u>>16); var b3 = (byte)(u>>24);
        if (offset == 0)  { v.B0=b0; v.B1=b1; v.B2=b2; v.B3=b3; }
        else if (offset == 4) { v.B4=b0; v.B5=b1; v.B6=b2; v.B7=b3; }
        else if (offset == 8) { v.B8=b0; v.B9=b1; v.B10=b2; v.B11=b3; }
        else { v.B12=b0; v.B13=b1; v.B14=b2; v.B15=b3; }
        return v;
    }
    private static V128Polyfill ApplyF32x4Binary(V128Polyfill a, V128Polyfill b, Func<float, float, float> op)
    {
        V128Polyfill r = default;
        for (var i = 0; i < 4; i++) r = SetF32(r, i*4, op(GetF32(a, i*4), GetF32(b, i*4)));
        return r;
    }
    private static V128Polyfill ApplyF32x4Unary(V128Polyfill a, Func<float, float> op)
    {
        V128Polyfill r = default;
        for (var i = 0; i < 4; i++) r = SetF32(r, i*4, op(GetF32(a, i*4)));
        return r;
    }
    public static V128Polyfill Float32x4Abs(V128Polyfill a) => ApplyF32x4Unary(a, x => x < 0 ? -x : x);
    public static V128Polyfill Float32x4Neg(V128Polyfill a) => ApplyF32x4Unary(a, x => -x);
    public static V128Polyfill Float32x4Sqrt(V128Polyfill a) => ApplyF32x4Unary(a, x => (float)Math.Sqrt(x));
    public static V128Polyfill Float32x4Ceil(V128Polyfill a) => ApplyF32x4Unary(a, x => (float)Math.Ceiling(x));
    public static V128Polyfill Float32x4Floor(V128Polyfill a) => ApplyF32x4Unary(a, x => (float)Math.Floor(x));
    public static V128Polyfill Float32x4Trunc(V128Polyfill a) => ApplyF32x4Unary(a, x => (float)Math.Truncate(x));
    public static V128Polyfill Float32x4Nearest(V128Polyfill a) => ApplyF32x4Unary(a, x => (float)Math.Round(x, MidpointRounding.ToEven));
    public static V128Polyfill Float32x4Add(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => x + y);
    public static V128Polyfill Float32x4Sub(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => x - y);
    public static V128Polyfill Float32x4Mul(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => x * y);
    public static V128Polyfill Float32x4Div(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => x / y);
    public static V128Polyfill Float32x4Min(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => x < y ? x : y);
    public static V128Polyfill Float32x4Max(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => x > y ? x : y);
    public static V128Polyfill Float32x4Pmin(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => y < x ? y : x);
    public static V128Polyfill Float32x4Pmax(V128Polyfill a, V128Polyfill b) => ApplyF32x4Binary(a, b, (x, y) => y > x ? y : x);

    // f64x2 helpers
    private static double GetF64Lo(V128Polyfill v)
    {
        var u = (ulong)v.B0|((ulong)v.B1<<8)|((ulong)v.B2<<16)|((ulong)v.B3<<24)|((ulong)v.B4<<32)|((ulong)v.B5<<40)|((ulong)v.B6<<48)|((ulong)v.B7<<56);
        return Unsafe.As<ulong, double>(ref u);
    }
    private static double GetF64Hi(V128Polyfill v)
    {
        var u = (ulong)v.B8|((ulong)v.B9<<8)|((ulong)v.B10<<16)|((ulong)v.B11<<24)|((ulong)v.B12<<32)|((ulong)v.B13<<40)|((ulong)v.B14<<48)|((ulong)v.B15<<56);
        return Unsafe.As<ulong, double>(ref u);
    }
    private static V128Polyfill SetF64(V128Polyfill v, bool hi, double d)
    {
        var u = Unsafe.As<double, ulong>(ref d);
        var b = new byte[8];
        for (var i = 0; i < 8; i++) b[i] = (byte)(u >> (i*8));
        if (!hi) { v.B0=b[0];v.B1=b[1];v.B2=b[2];v.B3=b[3];v.B4=b[4];v.B5=b[5];v.B6=b[6];v.B7=b[7]; }
        else      { v.B8=b[0];v.B9=b[1];v.B10=b[2];v.B11=b[3];v.B12=b[4];v.B13=b[5];v.B14=b[6];v.B15=b[7]; }
        return v;
    }
    private static V128Polyfill ApplyF64x2Binary(V128Polyfill a, V128Polyfill b, Func<double, double, double> op)
    {
        V128Polyfill r = default;
        r = SetF64(r, false, op(GetF64Lo(a), GetF64Lo(b)));
        r = SetF64(r, true,  op(GetF64Hi(a), GetF64Hi(b)));
        return r;
    }
    private static V128Polyfill ApplyF64x2Unary(V128Polyfill a, Func<double, double> op)
    {
        V128Polyfill r = default;
        r = SetF64(r, false, op(GetF64Lo(a)));
        r = SetF64(r, true,  op(GetF64Hi(a)));
        return r;
    }
    public static V128Polyfill Float64x2Abs(V128Polyfill a) => ApplyF64x2Unary(a, Math.Abs);
    public static V128Polyfill Float64x2Neg(V128Polyfill a) => ApplyF64x2Unary(a, x => -x);
    public static V128Polyfill Float64x2Sqrt(V128Polyfill a) => ApplyF64x2Unary(a, Math.Sqrt);
    public static V128Polyfill Float64x2Ceil(V128Polyfill a) => ApplyF64x2Unary(a, Math.Ceiling);
    public static V128Polyfill Float64x2Floor(V128Polyfill a) => ApplyF64x2Unary(a, Math.Floor);
    public static V128Polyfill Float64x2Trunc(V128Polyfill a) => ApplyF64x2Unary(a, Math.Truncate);
    public static V128Polyfill Float64x2Nearest(V128Polyfill a) => ApplyF64x2Unary(a, x => Math.Round(x, MidpointRounding.ToEven));
    public static V128Polyfill Float64x2Add(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, (x, y) => x + y);
    public static V128Polyfill Float64x2Sub(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, (x, y) => x - y);
    public static V128Polyfill Float64x2Mul(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, (x, y) => x * y);
    public static V128Polyfill Float64x2Div(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, (x, y) => x / y);
    public static V128Polyfill Float64x2Min(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, Math.Min);
    public static V128Polyfill Float64x2Max(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, Math.Max);
    public static V128Polyfill Float64x2Pmin(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, (x, y) => y < x ? y : x);
    public static V128Polyfill Float64x2Pmax(V128Polyfill a, V128Polyfill b) => ApplyF64x2Binary(a, b, (x, y) => y > x ? y : x);
#pragma warning restore CS1591
#endif
}

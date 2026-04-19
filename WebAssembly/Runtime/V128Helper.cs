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

    // --- comparisons ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16EqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Equal), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16NotEqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16NotEqual), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16LtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16LtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16LtUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16LtU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16GtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16GtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16GtUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16GtU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16LeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16LeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16LeUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16LeU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16GeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16GeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16GeUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16GeU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8EqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Equal), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8NotEqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8NotEqual), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8LtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8LtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8LtUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8LtU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8GtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8GtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8GtUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8GtU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8LeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8LeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8LeUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8LeU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8GeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8GeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8GeUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8GeU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4EqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Equal), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4NotEqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4NotEqual), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4LtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4LtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4LtUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4LtU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4GtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4GtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4GtUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4GtU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4LeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4LeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4LeUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4LeU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4GeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4GeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4GeUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4GeU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2EqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Equal), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2NotEqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2NotEqual), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2LtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2LtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2GtSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2GtS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2LeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2LeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2GeSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2GeS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4EqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Equal), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4NotEqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4NotEqual), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4LtMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Lt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4GtMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Gt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4LeMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Le), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4GeMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4Ge), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2EqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Equal), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2NotEqualMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2NotEqual), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2LtMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Lt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2GtMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Gt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2LeMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Le), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2GeMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2Ge), BindingFlags.Public | BindingFlags.Static)!);

    // --- shifts ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ShlMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Shl), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ShrSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16ShrS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ShrUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16ShrU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ShlMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Shl), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ShrSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ShrS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ShrUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ShrU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ShlMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Shl), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ShrSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ShrS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ShrUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ShrU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ShlMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Shl), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ShrSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ShrS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ShrUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ShrU), BindingFlags.Public | BindingFlags.Static)!);

    // --- AllTrue / Bitmask / AnyTrue ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16AllTrueMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16AllTrue), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8AllTrueMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8AllTrue), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4AllTrueMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4AllTrue), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2AllTrueMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2AllTrue), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16BitmaskMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Bitmask), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8BitmaskMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Bitmask), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4BitmaskMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4Bitmask), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2BitmaskMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2Bitmask), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128AnyTrueMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128AnyTrue), BindingFlags.Public | BindingFlags.Static)!);

    // --- misc unary ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16PopcntMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16Popcnt), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16AvgrUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16AvgrU), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8AvgrUMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8AvgrU), BindingFlags.Public | BindingFlags.Static)!);

    // --- narrow ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16NarrowI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16NarrowI16x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16NarrowI16x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16NarrowI16x8U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8NarrowI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8NarrowI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8NarrowI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8NarrowI32x4U), BindingFlags.Public | BindingFlags.Static)!);

    // --- extend ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtLowI8x16SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtLowI8x16S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtHighI8x16SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtHighI8x16S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtLowI8x16UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtLowI8x16U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtHighI8x16UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtHighI8x16U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtLowI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtLowI16x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtHighI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtHighI16x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtLowI16x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtLowI16x8U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtHighI16x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtHighI16x8U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtLowI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtLowI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtHighI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtHighI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtLowI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtLowI32x4U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtHighI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtHighI32x4U), BindingFlags.Public | BindingFlags.Static)!);

    // --- extmul ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtmulLowI8x16SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtmulLowI8x16S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtmulHighI8x16SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtmulHighI8x16S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtmulLowI8x16UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtmulLowI8x16U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtmulHighI8x16UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtmulHighI8x16U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtmulLowI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtmulLowI16x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtmulHighI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtmulHighI16x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtmulLowI16x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtmulLowI16x8U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtmulHighI16x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtmulHighI16x8U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtmulLowI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtmulLowI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtmulHighI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtmulHighI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtmulLowI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtmulLowI32x4U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int64x2ExtmulHighI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int64x2ExtmulHighI32x4U), BindingFlags.Public | BindingFlags.Static)!);

    // --- extadd pairwise ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtaddPairwiseI8x16SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtaddPairwiseI8x16S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8ExtaddPairwiseI8x16UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8ExtaddPairwiseI8x16U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtaddPairwiseI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtaddPairwiseI16x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4ExtaddPairwiseI16x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4ExtaddPairwiseI16x8U), BindingFlags.Public | BindingFlags.Static)!);

    // --- Q15MulrSat / Dot ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int16x8Q15MulrSatSMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int16x8Q15MulrSatS), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4DotI16x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4DotI16x8S), BindingFlags.Public | BindingFlags.Static)!);

    // --- V128Bitselect ---
    internal static readonly RegeneratingWeakReference<MethodInfo> V128BitselectMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Bitselect), BindingFlags.Public | BindingFlags.Static)!);

    // --- trunc sat / convert / demote / promote ---
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4TruncSatF32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4TruncSatF32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4TruncSatF32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4TruncSatF32x4U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4TruncSatF64x2SZeroMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4TruncSatF64x2SZero), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Int32x4TruncSatF64x2UZeroMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int32x4TruncSatF64x2UZero), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4ConvertI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4ConvertI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4ConvertI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4ConvertI32x4U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2ConvertLowI32x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2ConvertLowI32x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2ConvertLowI32x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2ConvertLowI32x4U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float32x4DemoteF64x2ZeroMethod = new(() => typeof(V128Helper).GetMethod(nameof(Float32x4DemoteF64x2Zero), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> Float64x2PromoteLowF32x4Method = new(() => typeof(V128Helper).GetMethod(nameof(Float64x2PromoteLowF32x4), BindingFlags.Public | BindingFlags.Static)!);

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

#pragma warning disable CS1591
    // --- comparisons (NET5+) ---
    public static Vector128<byte> Int8x16Equal(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a, b);
    public static Vector128<byte> Int8x16NotEqual(Vector128<byte> a, Vector128<byte> b) => ~Vector128.Equals(a, b);
    public static Vector128<byte> Int8x16LtS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsSByte(), b.AsSByte()).AsByte();
    public static Vector128<byte> Int8x16LtU(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a, b);
    public static Vector128<byte> Int8x16GtS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsSByte(), b.AsSByte()).AsByte();
    public static Vector128<byte> Int8x16GtU(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a, b);
    public static Vector128<byte> Int8x16LeS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsSByte(), b.AsSByte()).AsByte();
    public static Vector128<byte> Int8x16LeU(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a, b);
    public static Vector128<byte> Int8x16GeS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsSByte(), b.AsSByte()).AsByte();
    public static Vector128<byte> Int8x16GeU(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a, b);
    public static Vector128<byte> Int16x8Equal(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsInt16(), b.AsInt16()).AsByte();
    public static Vector128<byte> Int16x8NotEqual(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsInt16(), b.AsInt16())).AsByte();
    public static Vector128<byte> Int16x8LtS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsInt16(), b.AsInt16()).AsByte();
    public static Vector128<byte> Int16x8LtU(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsUInt16(), b.AsUInt16()).AsByte();
    public static Vector128<byte> Int16x8GtS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsInt16(), b.AsInt16()).AsByte();
    public static Vector128<byte> Int16x8GtU(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsUInt16(), b.AsUInt16()).AsByte();
    public static Vector128<byte> Int16x8LeS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsInt16(), b.AsInt16()).AsByte();
    public static Vector128<byte> Int16x8LeU(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsUInt16(), b.AsUInt16()).AsByte();
    public static Vector128<byte> Int16x8GeS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsInt16(), b.AsInt16()).AsByte();
    public static Vector128<byte> Int16x8GeU(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsUInt16(), b.AsUInt16()).AsByte();
    public static Vector128<byte> Int32x4Equal(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsInt32(), b.AsInt32()).AsByte();
    public static Vector128<byte> Int32x4NotEqual(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsInt32(), b.AsInt32())).AsByte();
    public static Vector128<byte> Int32x4LtS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsInt32(), b.AsInt32()).AsByte();
    public static Vector128<byte> Int32x4LtU(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsUInt32(), b.AsUInt32()).AsByte();
    public static Vector128<byte> Int32x4GtS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsInt32(), b.AsInt32()).AsByte();
    public static Vector128<byte> Int32x4GtU(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsUInt32(), b.AsUInt32()).AsByte();
    public static Vector128<byte> Int32x4LeS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsInt32(), b.AsInt32()).AsByte();
    public static Vector128<byte> Int32x4LeU(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsUInt32(), b.AsUInt32()).AsByte();
    public static Vector128<byte> Int32x4GeS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsInt32(), b.AsInt32()).AsByte();
    public static Vector128<byte> Int32x4GeU(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsUInt32(), b.AsUInt32()).AsByte();
    public static Vector128<byte> Int64x2Equal(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsInt64(), b.AsInt64()).AsByte();
    public static Vector128<byte> Int64x2NotEqual(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsInt64(), b.AsInt64())).AsByte();
    public static Vector128<byte> Int64x2LtS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsInt64(), b.AsInt64()).AsByte();
    public static Vector128<byte> Int64x2GtS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsInt64(), b.AsInt64()).AsByte();
    public static Vector128<byte> Int64x2LeS(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsInt64(), b.AsInt64()).AsByte();
    public static Vector128<byte> Int64x2GeS(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsInt64(), b.AsInt64()).AsByte();
    public static Vector128<byte> Float32x4Equal(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsSingle(), b.AsSingle()).AsByte();
    public static Vector128<byte> Float32x4NotEqual(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsSingle(), b.AsSingle())).AsByte();
    public static Vector128<byte> Float32x4Lt(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsSingle(), b.AsSingle()).AsByte();
    public static Vector128<byte> Float32x4Gt(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsSingle(), b.AsSingle()).AsByte();
    public static Vector128<byte> Float32x4Le(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsSingle(), b.AsSingle()).AsByte();
    public static Vector128<byte> Float32x4Ge(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsSingle(), b.AsSingle()).AsByte();
    public static Vector128<byte> Float64x2Equal(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsDouble(), b.AsDouble()).AsByte();
    public static Vector128<byte> Float64x2NotEqual(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsDouble(), b.AsDouble())).AsByte();
    public static Vector128<byte> Float64x2Lt(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsDouble(), b.AsDouble()).AsByte();
    public static Vector128<byte> Float64x2Gt(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsDouble(), b.AsDouble()).AsByte();
    public static Vector128<byte> Float64x2Le(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsDouble(), b.AsDouble()).AsByte();
    public static Vector128<byte> Float64x2Ge(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsDouble(), b.AsDouble()).AsByte();

    // --- shifts (NET5+) ---
    public static Vector128<byte> Int8x16Shl(Vector128<byte> a, int shift) { shift &= 7; var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)(a.GetElement(i) << shift); return Vector128.Create(r); }
    public static Vector128<byte> Int8x16ShrS(Vector128<byte> a, int shift) { shift &= 7; var r = new sbyte[16]; for (var i = 0; i < 16; i++) r[i] = (sbyte)(a.AsSByte().GetElement(i) >> shift); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int8x16ShrU(Vector128<byte> a, int shift) { shift &= 7; var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)(a.GetElement(i) >> shift); return Vector128.Create(r); }
    public static Vector128<byte> Int16x8Shl(Vector128<byte> a, int shift) => Vector128.ShiftLeft(a.AsInt16(), shift & 15).AsByte();
    public static Vector128<byte> Int16x8ShrS(Vector128<byte> a, int shift) => Vector128.ShiftRightArithmetic(a.AsInt16(), shift & 15).AsByte();
    public static Vector128<byte> Int16x8ShrU(Vector128<byte> a, int shift) => Vector128.ShiftRightLogical(a.AsUInt16(), shift & 15).AsByte();
    public static Vector128<byte> Int32x4Shl(Vector128<byte> a, int shift) => Vector128.ShiftLeft(a.AsInt32(), shift & 31).AsByte();
    public static Vector128<byte> Int32x4ShrS(Vector128<byte> a, int shift) => Vector128.ShiftRightArithmetic(a.AsInt32(), shift & 31).AsByte();
    public static Vector128<byte> Int32x4ShrU(Vector128<byte> a, int shift) => Vector128.ShiftRightLogical(a.AsUInt32(), shift & 31).AsByte();
    public static Vector128<byte> Int64x2Shl(Vector128<byte> a, int shift) => Vector128.ShiftLeft(a.AsInt64(), shift & 63).AsByte();
    public static Vector128<byte> Int64x2ShrS(Vector128<byte> a, int shift) => Vector128.ShiftRightArithmetic(a.AsInt64(), shift & 63).AsByte();
    public static Vector128<byte> Int64x2ShrU(Vector128<byte> a, int shift) => Vector128.ShiftRightLogical(a.AsUInt64(), shift & 63).AsByte();

    // --- AllTrue / Bitmask / AnyTrue (NET5+) ---
    public static int V128AnyTrue(Vector128<byte> a) => Vector128.EqualsAll(a, Vector128<byte>.Zero) ? 0 : 1;
    public static int Int8x16AllTrue(Vector128<byte> a) => Vector128.EqualsAny(a, Vector128<byte>.Zero) ? 0 : 1;
    public static int Int16x8AllTrue(Vector128<byte> a) => Vector128.EqualsAny(a.AsInt16(), Vector128<short>.Zero) ? 0 : 1;
    public static int Int32x4AllTrue(Vector128<byte> a) => Vector128.EqualsAny(a.AsInt32(), Vector128<int>.Zero) ? 0 : 1;
    public static int Int64x2AllTrue(Vector128<byte> a) => Vector128.EqualsAny(a.AsInt64(), Vector128<long>.Zero) ? 0 : 1;
    public static int Int8x16Bitmask(Vector128<byte> a) { var r = 0; for (var i = 0; i < 16; i++) if ((a.GetElement(i) >> 7) != 0) r |= 1 << i; return r; }
    public static int Int16x8Bitmask(Vector128<byte> a) { var r = 0; for (var i = 0; i < 8; i++) if ((a.AsUInt16().GetElement(i) >> 15) != 0) r |= 1 << i; return r; }
    public static int Int32x4Bitmask(Vector128<byte> a) { var r = 0; for (var i = 0; i < 4; i++) if (((uint)a.AsInt32().GetElement(i) >> 31) != 0) r |= 1 << i; return r; }
    public static int Int64x2Bitmask(Vector128<byte> a) { var r = 0; for (var i = 0; i < 2; i++) if (((ulong)a.AsInt64().GetElement(i) >> 63) != 0) r |= 1 << i; return r; }

    // --- misc unary (NET5+) ---
    public static Vector128<byte> Int8x16Popcnt(Vector128<byte> a) { var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)System.Numerics.BitOperations.PopCount(a.GetElement(i)); return Vector128.Create(r); }
    public static Vector128<byte> Int8x16AvgrU(Vector128<byte> a, Vector128<byte> b) { var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)((a.GetElement(i) + b.GetElement(i) + 1) >> 1); return Vector128.Create(r); }
    public static Vector128<byte> Int16x8AvgrU(Vector128<byte> a, Vector128<byte> b) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)((a.AsUInt16().GetElement(i) + b.AsUInt16().GetElement(i) + 1) >> 1); return Vector128.Create(r).AsByte(); }

    // --- narrow (NET5+) ---
    public static Vector128<byte> Int8x16NarrowI16x8S(Vector128<byte> a, Vector128<byte> b) { var r = new sbyte[16]; for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i); r[i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; } for (var i = 0; i < 8; i++) { var v = b.AsInt16().GetElement(i); r[8+i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int8x16NarrowI16x8U(Vector128<byte> a, Vector128<byte> b) { var r = new byte[16]; for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i); r[i] = v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v; } for (var i = 0; i < 8; i++) { var v = b.AsInt16().GetElement(i); r[8+i] = v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v; } return Vector128.Create(r); }
    public static Vector128<byte> Int16x8NarrowI32x4S(Vector128<byte> a, Vector128<byte> b) { var r = new short[8]; for (var i = 0; i < 4; i++) { var v = a.AsInt32().GetElement(i); r[i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; } for (var i = 0; i < 4; i++) { var v = b.AsInt32().GetElement(i); r[4+i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8NarrowI32x4U(Vector128<byte> a, Vector128<byte> b) { var r = new ushort[8]; for (var i = 0; i < 4; i++) { var v = a.AsInt32().GetElement(i); r[i] = v < 0 ? (ushort)0 : v > 65535 ? (ushort)65535 : (ushort)v; } for (var i = 0; i < 4; i++) { var v = b.AsInt32().GetElement(i); r[4+i] = v < 0 ? (ushort)0 : v > 65535 ? (ushort)65535 : (ushort)v; } return Vector128.Create(r).AsByte(); }

    // --- extend (NET5+) ---
    public static Vector128<byte> Int16x8ExtLowI8x16S(Vector128<byte> a) { var r = new short[8]; for (var i = 0; i < 8; i++) r[i] = (sbyte)a.GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtHighI8x16S(Vector128<byte> a) { var r = new short[8]; for (var i = 0; i < 8; i++) r[i] = (sbyte)a.GetElement(8+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtLowI8x16U(Vector128<byte> a) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = a.GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtHighI8x16U(Vector128<byte> a) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = a.GetElement(8+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtLowI16x8S(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtHighI16x8S(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(4+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtLowI16x8U(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = a.AsUInt16().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtHighI16x8U(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = a.AsUInt16().GetElement(4+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtLowI32x4S(Vector128<byte> a) { var r = new long[2]; for (var i = 0; i < 2; i++) r[i] = a.AsInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtHighI32x4S(Vector128<byte> a) { var r = new long[2]; for (var i = 0; i < 2; i++) r[i] = a.AsInt32().GetElement(2+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtLowI32x4U(Vector128<byte> a) { var r = new ulong[2]; for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtHighI32x4U(Vector128<byte> a) { var r = new ulong[2]; for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(2+i); return Vector128.Create(r).AsByte(); }

    // --- extmul (NET5+) ---
    public static Vector128<byte> Int16x8ExtmulLowI8x16S(Vector128<byte> a, Vector128<byte> b) { var r = new short[8]; for (var i = 0; i < 8; i++) r[i] = (short)((sbyte)a.GetElement(i) * (sbyte)b.GetElement(i)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtmulHighI8x16S(Vector128<byte> a, Vector128<byte> b) { var r = new short[8]; for (var i = 0; i < 8; i++) r[i] = (short)((sbyte)a.GetElement(8+i) * (sbyte)b.GetElement(8+i)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtmulLowI8x16U(Vector128<byte> a, Vector128<byte> b) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)(a.GetElement(i) * b.GetElement(i)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtmulHighI8x16U(Vector128<byte> a, Vector128<byte> b) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)(a.GetElement(8+i) * b.GetElement(8+i)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtmulLowI16x8S(Vector128<byte> a, Vector128<byte> b) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(i) * b.AsInt16().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtmulHighI16x8S(Vector128<byte> a, Vector128<byte> b) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(4+i) * b.AsInt16().GetElement(4+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtmulLowI16x8U(Vector128<byte> a, Vector128<byte> b) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = (uint)(a.AsUInt16().GetElement(i) * b.AsUInt16().GetElement(i)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtmulHighI16x8U(Vector128<byte> a, Vector128<byte> b) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = (uint)(a.AsUInt16().GetElement(4+i) * b.AsUInt16().GetElement(4+i)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtmulLowI32x4S(Vector128<byte> a, Vector128<byte> b) { var r = new long[2]; for (var i = 0; i < 2; i++) r[i] = (long)a.AsInt32().GetElement(i) * b.AsInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtmulHighI32x4S(Vector128<byte> a, Vector128<byte> b) { var r = new long[2]; for (var i = 0; i < 2; i++) r[i] = (long)a.AsInt32().GetElement(2+i) * b.AsInt32().GetElement(2+i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtmulLowI32x4U(Vector128<byte> a, Vector128<byte> b) { var r = new ulong[2]; for (var i = 0; i < 2; i++) r[i] = (ulong)a.AsUInt32().GetElement(i) * b.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int64x2ExtmulHighI32x4U(Vector128<byte> a, Vector128<byte> b) { var r = new ulong[2]; for (var i = 0; i < 2; i++) r[i] = (ulong)a.AsUInt32().GetElement(2+i) * b.AsUInt32().GetElement(2+i); return Vector128.Create(r).AsByte(); }

    // --- extadd pairwise (NET5+) ---
    public static Vector128<byte> Int16x8ExtaddPairwiseI8x16S(Vector128<byte> a) { var r = new short[8]; for (var i = 0; i < 8; i++) r[i] = (short)((sbyte)a.GetElement(i*2) + (sbyte)a.GetElement(i*2+1)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int16x8ExtaddPairwiseI8x16U(Vector128<byte> a) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)(a.GetElement(i*2) + a.GetElement(i*2+1)); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtaddPairwiseI16x8S(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(i*2) + a.AsInt16().GetElement(i*2+1); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4ExtaddPairwiseI16x8U(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = (uint)(a.AsUInt16().GetElement(i*2) + a.AsUInt16().GetElement(i*2+1)); return Vector128.Create(r).AsByte(); }

    // --- Q15MulrSat / Dot (NET5+) ---
    public static Vector128<byte> Int16x8Q15MulrSatS(Vector128<byte> a, Vector128<byte> b) { var r = new short[8]; for (var i = 0; i < 8; i++) { var v = ((int)a.AsInt16().GetElement(i) * b.AsInt16().GetElement(i) + 0x4000) >> 15; r[i] = v > 32767 ? (short)32767 : (short)v; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4DotI16x8S(Vector128<byte> a, Vector128<byte> b) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(i*2) * b.AsInt16().GetElement(i*2) + a.AsInt16().GetElement(i*2+1) * b.AsInt16().GetElement(i*2+1); return Vector128.Create(r).AsByte(); }

    // --- bitselect (NET5+) ---
    public static Vector128<byte> V128Bitselect(Vector128<byte> v1, Vector128<byte> v2, Vector128<byte> mask) => Vector128.ConditionalSelect(mask, v1, v2);

    // --- trunc sat / convert / demote / promote (NET5+) ---
    public static Vector128<byte> Int32x4TruncSatF32x4S(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 4; i++) { var f = a.AsSingle().GetElement(i); r[i] = float.IsNaN(f) ? 0 : f >= 2147483647f ? int.MaxValue : f <= -2147483648f ? int.MinValue : (int)f; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4TruncSatF32x4U(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 4; i++) { var f = a.AsSingle().GetElement(i); r[i] = float.IsNaN(f) || f < 0 ? 0u : f >= 4294967295f ? uint.MaxValue : (uint)f; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4TruncSatF64x2SZero(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 2; i++) { var f = a.AsDouble().GetElement(i); r[i] = double.IsNaN(f) ? 0 : f >= 2147483647d ? int.MaxValue : f <= -2147483648d ? int.MinValue : (int)f; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Int32x4TruncSatF64x2UZero(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 2; i++) { var f = a.AsDouble().GetElement(i); r[i] = double.IsNaN(f) || f < 0 ? 0u : f >= 4294967295d ? uint.MaxValue : (uint)f; } return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Float32x4ConvertI32x4S(Vector128<byte> a) { var r = new float[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Float32x4ConvertI32x4U(Vector128<byte> a) { var r = new float[4]; for (var i = 0; i < 4; i++) r[i] = a.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Float64x2ConvertLowI32x4S(Vector128<byte> a) { var r = new double[2]; for (var i = 0; i < 2; i++) r[i] = a.AsInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Float64x2ConvertLowI32x4U(Vector128<byte> a) { var r = new double[2]; for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Float32x4DemoteF64x2Zero(Vector128<byte> a) { var r = new float[4]; r[0] = (float)a.AsDouble().GetElement(0); r[1] = (float)a.AsDouble().GetElement(1); return Vector128.Create(r).AsByte(); }
    public static Vector128<byte> Float64x2PromoteLowF32x4(Vector128<byte> a) { var r = new double[2]; r[0] = a.AsSingle().GetElement(0); r[1] = a.AsSingle().GetElement(1); return Vector128.Create(r).AsByte(); }
#pragma warning restore CS1591
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

    // comparisons (polyfill) — all-ones = 0xFF per lane if true
    private static byte Mask8(bool c) => c ? (byte)0xFF : (byte)0;
    private static V128Polyfill CmpI8(V128Polyfill a, V128Polyfill b, Func<sbyte, sbyte, bool> op) => new() { B0=Mask8(op((sbyte)a.B0,(sbyte)b.B0)),B1=Mask8(op((sbyte)a.B1,(sbyte)b.B1)),B2=Mask8(op((sbyte)a.B2,(sbyte)b.B2)),B3=Mask8(op((sbyte)a.B3,(sbyte)b.B3)),B4=Mask8(op((sbyte)a.B4,(sbyte)b.B4)),B5=Mask8(op((sbyte)a.B5,(sbyte)b.B5)),B6=Mask8(op((sbyte)a.B6,(sbyte)b.B6)),B7=Mask8(op((sbyte)a.B7,(sbyte)b.B7)),B8=Mask8(op((sbyte)a.B8,(sbyte)b.B8)),B9=Mask8(op((sbyte)a.B9,(sbyte)b.B9)),B10=Mask8(op((sbyte)a.B10,(sbyte)b.B10)),B11=Mask8(op((sbyte)a.B11,(sbyte)b.B11)),B12=Mask8(op((sbyte)a.B12,(sbyte)b.B12)),B13=Mask8(op((sbyte)a.B13,(sbyte)b.B13)),B14=Mask8(op((sbyte)a.B14,(sbyte)b.B14)),B15=Mask8(op((sbyte)a.B15,(sbyte)b.B15)) };
    private static V128Polyfill CmpU8(V128Polyfill a, V128Polyfill b, Func<byte, byte, bool> op) => new() { B0=Mask8(op(a.B0,b.B0)),B1=Mask8(op(a.B1,b.B1)),B2=Mask8(op(a.B2,b.B2)),B3=Mask8(op(a.B3,b.B3)),B4=Mask8(op(a.B4,b.B4)),B5=Mask8(op(a.B5,b.B5)),B6=Mask8(op(a.B6,b.B6)),B7=Mask8(op(a.B7,b.B7)),B8=Mask8(op(a.B8,b.B8)),B9=Mask8(op(a.B9,b.B9)),B10=Mask8(op(a.B10,b.B10)),B11=Mask8(op(a.B11,b.B11)),B12=Mask8(op(a.B12,b.B12)),B13=Mask8(op(a.B13,b.B13)),B14=Mask8(op(a.B14,b.B14)),B15=Mask8(op(a.B15,b.B15)) };
    private static V128Polyfill CmpI16(V128Polyfill a, V128Polyfill b, Func<short, short, bool> op) => ApplyI16Binary(a, b, (x, y) => op(x, y) ? (short)-1 : (short)0);
    private static V128Polyfill CmpU16(V128Polyfill a, V128Polyfill b, Func<ushort, ushort, bool> op) => ApplyI16Binary(a, b, (x, y) => op((ushort)x, (ushort)y) ? (short)-1 : (short)0);
    private static V128Polyfill CmpI32(V128Polyfill a, V128Polyfill b, Func<int, int, bool> op) => ApplyI32Binary(a, b, (x, y) => op(x, y) ? -1 : 0);
    private static V128Polyfill CmpU32(V128Polyfill a, V128Polyfill b, Func<uint, uint, bool> op) => ApplyI32Binary(a, b, (x, y) => op((uint)x, (uint)y) ? -1 : 0);
    private static V128Polyfill CmpI64(V128Polyfill a, V128Polyfill b, Func<long, long, bool> op) => ApplyI64Binary(a, b, (x, y) => op(x, y) ? -1L : 0L);
    private static V128Polyfill CmpF32(V128Polyfill a, V128Polyfill b, Func<float, float, bool> op) => ApplyF32x4Binary(a, b, (x, y) => op(x, y) ? BitConverter.Int32BitsToSingle(-1) : 0f);
    private static V128Polyfill CmpF64(V128Polyfill a, V128Polyfill b, Func<double, double, bool> op) => ApplyF64x2Binary(a, b, (x, y) => op(x, y) ? BitConverter.Int64BitsToDouble(-1L) : 0d);
    public static V128Polyfill Int8x16Equal(V128Polyfill a, V128Polyfill b) => CmpI8(a, b, (x, y) => x == y);
    public static V128Polyfill Int8x16NotEqual(V128Polyfill a, V128Polyfill b) => CmpI8(a, b, (x, y) => x != y);
    public static V128Polyfill Int8x16LtS(V128Polyfill a, V128Polyfill b) => CmpI8(a, b, (x, y) => x < y);
    public static V128Polyfill Int8x16LtU(V128Polyfill a, V128Polyfill b) => CmpU8(a, b, (x, y) => x < y);
    public static V128Polyfill Int8x16GtS(V128Polyfill a, V128Polyfill b) => CmpI8(a, b, (x, y) => x > y);
    public static V128Polyfill Int8x16GtU(V128Polyfill a, V128Polyfill b) => CmpU8(a, b, (x, y) => x > y);
    public static V128Polyfill Int8x16LeS(V128Polyfill a, V128Polyfill b) => CmpI8(a, b, (x, y) => x <= y);
    public static V128Polyfill Int8x16LeU(V128Polyfill a, V128Polyfill b) => CmpU8(a, b, (x, y) => x <= y);
    public static V128Polyfill Int8x16GeS(V128Polyfill a, V128Polyfill b) => CmpI8(a, b, (x, y) => x >= y);
    public static V128Polyfill Int8x16GeU(V128Polyfill a, V128Polyfill b) => CmpU8(a, b, (x, y) => x >= y);
    public static V128Polyfill Int16x8Equal(V128Polyfill a, V128Polyfill b) => CmpI16(a, b, (x, y) => x == y);
    public static V128Polyfill Int16x8NotEqual(V128Polyfill a, V128Polyfill b) => CmpI16(a, b, (x, y) => x != y);
    public static V128Polyfill Int16x8LtS(V128Polyfill a, V128Polyfill b) => CmpI16(a, b, (x, y) => x < y);
    public static V128Polyfill Int16x8LtU(V128Polyfill a, V128Polyfill b) => CmpU16(a, b, (x, y) => x < y);
    public static V128Polyfill Int16x8GtS(V128Polyfill a, V128Polyfill b) => CmpI16(a, b, (x, y) => x > y);
    public static V128Polyfill Int16x8GtU(V128Polyfill a, V128Polyfill b) => CmpU16(a, b, (x, y) => x > y);
    public static V128Polyfill Int16x8LeS(V128Polyfill a, V128Polyfill b) => CmpI16(a, b, (x, y) => x <= y);
    public static V128Polyfill Int16x8LeU(V128Polyfill a, V128Polyfill b) => CmpU16(a, b, (x, y) => x <= y);
    public static V128Polyfill Int16x8GeS(V128Polyfill a, V128Polyfill b) => CmpI16(a, b, (x, y) => x >= y);
    public static V128Polyfill Int16x8GeU(V128Polyfill a, V128Polyfill b) => CmpU16(a, b, (x, y) => x >= y);
    public static V128Polyfill Int32x4Equal(V128Polyfill a, V128Polyfill b) => CmpI32(a, b, (x, y) => x == y);
    public static V128Polyfill Int32x4NotEqual(V128Polyfill a, V128Polyfill b) => CmpI32(a, b, (x, y) => x != y);
    public static V128Polyfill Int32x4LtS(V128Polyfill a, V128Polyfill b) => CmpI32(a, b, (x, y) => x < y);
    public static V128Polyfill Int32x4LtU(V128Polyfill a, V128Polyfill b) => CmpU32(a, b, (x, y) => x < y);
    public static V128Polyfill Int32x4GtS(V128Polyfill a, V128Polyfill b) => CmpI32(a, b, (x, y) => x > y);
    public static V128Polyfill Int32x4GtU(V128Polyfill a, V128Polyfill b) => CmpU32(a, b, (x, y) => x > y);
    public static V128Polyfill Int32x4LeS(V128Polyfill a, V128Polyfill b) => CmpI32(a, b, (x, y) => x <= y);
    public static V128Polyfill Int32x4LeU(V128Polyfill a, V128Polyfill b) => CmpU32(a, b, (x, y) => x <= y);
    public static V128Polyfill Int32x4GeS(V128Polyfill a, V128Polyfill b) => CmpI32(a, b, (x, y) => x >= y);
    public static V128Polyfill Int32x4GeU(V128Polyfill a, V128Polyfill b) => CmpU32(a, b, (x, y) => x >= y);
    public static V128Polyfill Int64x2Equal(V128Polyfill a, V128Polyfill b) => CmpI64(a, b, (x, y) => x == y);
    public static V128Polyfill Int64x2NotEqual(V128Polyfill a, V128Polyfill b) => CmpI64(a, b, (x, y) => x != y);
    public static V128Polyfill Int64x2LtS(V128Polyfill a, V128Polyfill b) => CmpI64(a, b, (x, y) => x < y);
    public static V128Polyfill Int64x2GtS(V128Polyfill a, V128Polyfill b) => CmpI64(a, b, (x, y) => x > y);
    public static V128Polyfill Int64x2LeS(V128Polyfill a, V128Polyfill b) => CmpI64(a, b, (x, y) => x <= y);
    public static V128Polyfill Int64x2GeS(V128Polyfill a, V128Polyfill b) => CmpI64(a, b, (x, y) => x >= y);
    public static V128Polyfill Float32x4Equal(V128Polyfill a, V128Polyfill b) => CmpF32(a, b, (x, y) => x == y);
    public static V128Polyfill Float32x4NotEqual(V128Polyfill a, V128Polyfill b) => CmpF32(a, b, (x, y) => x != y);
    public static V128Polyfill Float32x4Lt(V128Polyfill a, V128Polyfill b) => CmpF32(a, b, (x, y) => x < y);
    public static V128Polyfill Float32x4Gt(V128Polyfill a, V128Polyfill b) => CmpF32(a, b, (x, y) => x > y);
    public static V128Polyfill Float32x4Le(V128Polyfill a, V128Polyfill b) => CmpF32(a, b, (x, y) => x <= y);
    public static V128Polyfill Float32x4Ge(V128Polyfill a, V128Polyfill b) => CmpF32(a, b, (x, y) => x >= y);
    public static V128Polyfill Float64x2Equal(V128Polyfill a, V128Polyfill b) => CmpF64(a, b, (x, y) => x == y);
    public static V128Polyfill Float64x2NotEqual(V128Polyfill a, V128Polyfill b) => CmpF64(a, b, (x, y) => x != y);
    public static V128Polyfill Float64x2Lt(V128Polyfill a, V128Polyfill b) => CmpF64(a, b, (x, y) => x < y);
    public static V128Polyfill Float64x2Gt(V128Polyfill a, V128Polyfill b) => CmpF64(a, b, (x, y) => x > y);
    public static V128Polyfill Float64x2Le(V128Polyfill a, V128Polyfill b) => CmpF64(a, b, (x, y) => x <= y);
    public static V128Polyfill Float64x2Ge(V128Polyfill a, V128Polyfill b) => CmpF64(a, b, (x, y) => x >= y);

    // shifts (polyfill)
    public static V128Polyfill Int8x16Shl(V128Polyfill a, int s) { s &= 7; return ApplyUnary(a, b => (byte)(b << s)); }
    public static V128Polyfill Int8x16ShrS(V128Polyfill a, int s) { s &= 7; return ApplyUnary(a, b => (byte)(sbyte)((sbyte)b >> s)); }
    public static V128Polyfill Int8x16ShrU(V128Polyfill a, int s) { s &= 7; return ApplyUnary(a, b => (byte)(b >> s)); }
    public static V128Polyfill Int16x8Shl(V128Polyfill a, int s) { s &= 15; return ApplyI16Unary(a, x => (short)(x << s)); }
    public static V128Polyfill Int16x8ShrS(V128Polyfill a, int s) { s &= 15; return ApplyI16Unary(a, x => (short)(x >> s)); }
    public static V128Polyfill Int16x8ShrU(V128Polyfill a, int s) { s &= 15; return ApplyI16Unary(a, x => (short)((ushort)x >> s)); }
    public static V128Polyfill Int32x4Shl(V128Polyfill a, int s) { s &= 31; return ApplyI32Unary(a, x => x << s); }
    public static V128Polyfill Int32x4ShrS(V128Polyfill a, int s) { s &= 31; return ApplyI32Unary(a, x => x >> s); }
    public static V128Polyfill Int32x4ShrU(V128Polyfill a, int s) { s &= 31; return ApplyI32Unary(a, x => (int)((uint)x >> s)); }
    public static V128Polyfill Int64x2Shl(V128Polyfill a, int s) { s &= 63; return ApplyI64Unary(a, x => x << s); }
    public static V128Polyfill Int64x2ShrS(V128Polyfill a, int s) { s &= 63; return ApplyI64Unary(a, x => x >> s); }
    public static V128Polyfill Int64x2ShrU(V128Polyfill a, int s) { s &= 63; return ApplyI64Unary(a, x => (long)((ulong)x >> s)); }

    // AllTrue / Bitmask / AnyTrue (polyfill)
    public static int V128AnyTrue(V128Polyfill a) => (a.B0|a.B1|a.B2|a.B3|a.B4|a.B5|a.B6|a.B7|a.B8|a.B9|a.B10|a.B11|a.B12|a.B13|a.B14|a.B15) != 0 ? 1 : 0;
    public static int Int8x16AllTrue(V128Polyfill a) => (a.B0&a.B1&a.B2&a.B3&a.B4&a.B5&a.B6&a.B7&a.B8&a.B9&a.B10&a.B11&a.B12&a.B13&a.B14&a.B15) != 0 ? 1 : 0;
    public static int Int16x8AllTrue(V128Polyfill a) { for(var i=0;i<8;i++){var b=i*2;if((GetByte(a,b)|(GetByte(a,b+1)<<8))==0)return 0;} return 1; }
    public static int Int32x4AllTrue(V128Polyfill a) { for(var i=0;i<4;i++){var b=i*4;if((GetByte(a,b)|(GetByte(a,b+1)<<8)|(GetByte(a,b+2)<<16)|(GetByte(a,b+3)<<24))==0)return 0;} return 1; }
    public static int Int64x2AllTrue(V128Polyfill a) => (Int64x2ExtractLane(a,0)|Int64x2ExtractLane(a,1))!=0?1:0;
    public static int Int8x16Bitmask(V128Polyfill a) { var r=0; for(var i=0;i<16;i++) if((GetByte(a,i)>>7)!=0) r|=1<<i; return r; }
    public static int Int16x8Bitmask(V128Polyfill a) { var r=0; for(var i=0;i<8;i++) { var b=i*2; if(((GetByte(a,b)|(GetByte(a,b+1)<<8))>>15)!=0) r|=1<<i; } return r; }
    public static int Int32x4Bitmask(V128Polyfill a) { var r=0; for(var i=0;i<4;i++) { var b=i*4; if(((uint)(GetByte(a,b)|(GetByte(a,b+1)<<8)|(GetByte(a,b+2)<<16)|(GetByte(a,b+3)<<24))>>31)!=0) r|=1<<i; } return r; }
    public static int Int64x2Bitmask(V128Polyfill a) { var r=0; if(((ulong)Int64x2ExtractLane(a,0)>>63)!=0) r|=1; if(((ulong)Int64x2ExtractLane(a,1)>>63)!=0) r|=2; return r; }

    // misc unary (polyfill)
    public static V128Polyfill Int8x16Popcnt(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<16;i++){var b=GetByte(a,i);var c=0;while(b!=0){c+=b&1;b>>=1;}r[i]=(byte)c;} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int8x16AvgrU(V128Polyfill a, V128Polyfill b) => ApplyBinary(a, b, (x, y) => (byte)((x + y + 1) >> 1));
    public static V128Polyfill Int16x8AvgrU(V128Polyfill a, V128Polyfill b) => ApplyI16Binary(a, b, (x, y) => (short)(ushort)(((ushort)x + (ushort)y + 1) >> 1));

    // narrow (polyfill)
    public static V128Polyfill Int8x16NarrowI16x8S(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=Int16x8ExtractLaneS(a,i);r[i]=(byte)(sbyte)(v<-128?-128:v>127?127:v);} for(var i=0;i<8;i++){var v=Int16x8ExtractLaneS(b,i);r[8+i]=(byte)(sbyte)(v<-128?-128:v>127?127:v);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int8x16NarrowI16x8U(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=Int16x8ExtractLaneS(a,i);r[i]=v<0?(byte)0:v>255?(byte)255:(byte)v;} for(var i=0;i<8;i++){var v=Int16x8ExtractLaneS(b,i);r[8+i]=v<0?(byte)0:v>255?(byte)255:(byte)v;} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8NarrowI32x4S(V128Polyfill a, V128Polyfill b) { static short Clamp(int v)=>v<-32768?(short)-32768:v>32767?(short)32767:(short)v; var r=new byte[16]; for(var i=0;i<4;i++){var s=Clamp(Int32x4ExtractLane(a,i));r[i*2]=(byte)s;r[i*2+1]=(byte)((ushort)s>>8);} for(var i=0;i<4;i++){var s=Clamp(Int32x4ExtractLane(b,i));r[8+i*2]=(byte)s;r[8+i*2+1]=(byte)((ushort)s>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8NarrowI32x4U(V128Polyfill a, V128Polyfill b) { static ushort ClampU(int v)=>v<0?(ushort)0:v>65535?(ushort)65535:(ushort)v; var r=new byte[16]; for(var i=0;i<4;i++){var s=ClampU(Int32x4ExtractLane(a,i));r[i*2]=(byte)s;r[i*2+1]=(byte)(s>>8);} for(var i=0;i<4;i++){var s=ClampU(Int32x4ExtractLane(b,i));r[8+i*2]=(byte)s;r[8+i*2+1]=(byte)(s>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }

    // extend (polyfill)
    public static V128Polyfill Int16x8ExtLowI8x16S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(short)(sbyte)GetByte(a,i);r[i*2]=(byte)v;r[i*2+1]=(byte)((ushort)v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtHighI8x16S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(short)(sbyte)GetByte(a,8+i);r[i*2]=(byte)v;r[i*2+1]=(byte)((ushort)v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtLowI8x16U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(ushort)GetByte(a,i);r[i*2]=(byte)v;r[i*2+1]=(byte)(v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtHighI8x16U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(ushort)GetByte(a,8+i);r[i*2]=(byte)v;r[i*2+1]=(byte)(v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtLowI16x8S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(int)Int16x8ExtractLaneS(a,i);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtHighI16x8S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(int)Int16x8ExtractLaneS(a,4+i);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtLowI16x8U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(uint)Int16x8ExtractLaneU(a,i);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtHighI16x8U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(uint)Int16x8ExtractLaneU(a,4+i);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int64x2ExtLowI32x4S(V128Polyfill a) { var lo=(long)Int32x4ExtractLane(a,0); var hi=(long)Int32x4ExtractLane(a,1); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }
    public static V128Polyfill Int64x2ExtHighI32x4S(V128Polyfill a) { var lo=(long)Int32x4ExtractLane(a,2); var hi=(long)Int32x4ExtractLane(a,3); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }
    public static V128Polyfill Int64x2ExtLowI32x4U(V128Polyfill a) { var lo=(long)(uint)Int32x4ExtractLane(a,0); var hi=(long)(uint)Int32x4ExtractLane(a,1); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }
    public static V128Polyfill Int64x2ExtHighI32x4U(V128Polyfill a) { var lo=(long)(uint)Int32x4ExtractLane(a,2); var hi=(long)(uint)Int32x4ExtractLane(a,3); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }

    // extmul (polyfill)
    public static V128Polyfill Int16x8ExtmulLowI8x16S(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(short)((sbyte)GetByte(a,i)*(sbyte)GetByte(b,i));r[i*2]=(byte)v;r[i*2+1]=(byte)((ushort)v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtmulHighI8x16S(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(short)((sbyte)GetByte(a,8+i)*(sbyte)GetByte(b,8+i));r[i*2]=(byte)v;r[i*2+1]=(byte)((ushort)v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtmulLowI8x16U(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(ushort)(GetByte(a,i)*GetByte(b,i));r[i*2]=(byte)v;r[i*2+1]=(byte)(v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtmulHighI8x16U(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(ushort)(GetByte(a,8+i)*GetByte(b,8+i));r[i*2]=(byte)v;r[i*2+1]=(byte)(v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtmulLowI16x8S(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<4;i++){var v=Int16x8ExtractLaneS(a,i)*Int16x8ExtractLaneS(b,i);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtmulHighI16x8S(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<4;i++){var v=Int16x8ExtractLaneS(a,i)*Int16x8ExtractLaneS(b,i);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtmulLowI16x8U(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(uint)(Int16x8ExtractLaneU(a,i)*Int16x8ExtractLaneU(b,i));r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtmulHighI16x8U(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(uint)(Int16x8ExtractLaneU(a,4+i)*Int16x8ExtractLaneU(b,4+i));r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int64x2ExtmulLowI32x4S(V128Polyfill a, V128Polyfill b) { var lo=(long)Int32x4ExtractLane(a,0)*(long)Int32x4ExtractLane(b,0); var hi=(long)Int32x4ExtractLane(a,1)*(long)Int32x4ExtractLane(b,1); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }
    public static V128Polyfill Int64x2ExtmulHighI32x4S(V128Polyfill a, V128Polyfill b) { var lo=(long)Int32x4ExtractLane(a,2)*(long)Int32x4ExtractLane(b,2); var hi=(long)Int32x4ExtractLane(a,3)*(long)Int32x4ExtractLane(b,3); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }
    public static V128Polyfill Int64x2ExtmulLowI32x4U(V128Polyfill a, V128Polyfill b) { var lo=(long)((ulong)(uint)Int32x4ExtractLane(a,0)*(ulong)(uint)Int32x4ExtractLane(b,0)); var hi=(long)((ulong)(uint)Int32x4ExtractLane(a,1)*(ulong)(uint)Int32x4ExtractLane(b,1)); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }
    public static V128Polyfill Int64x2ExtmulHighI32x4U(V128Polyfill a, V128Polyfill b) { var lo=(long)((ulong)(uint)Int32x4ExtractLane(a,2)*(ulong)(uint)Int32x4ExtractLane(b,2)); var hi=(long)((ulong)(uint)Int32x4ExtractLane(a,3)*(ulong)(uint)Int32x4ExtractLane(b,3)); return Int64x2Splat(lo) with { B8=(byte)hi,B9=(byte)(hi>>8),B10=(byte)(hi>>16),B11=(byte)(hi>>24),B12=(byte)(hi>>32),B13=(byte)(hi>>40),B14=(byte)(hi>>48),B15=(byte)(hi>>56) }; }

    // extadd pairwise (polyfill)
    public static V128Polyfill Int16x8ExtaddPairwiseI8x16S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(short)((sbyte)GetByte(a,i*2)+(sbyte)GetByte(a,i*2+1));r[i*2]=(byte)v;r[i*2+1]=(byte)((ushort)v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int16x8ExtaddPairwiseI8x16U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<8;i++){var v=(ushort)(GetByte(a,i*2)+GetByte(a,i*2+1));r[i*2]=(byte)v;r[i*2+1]=(byte)(v>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtaddPairwiseI16x8S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var v=Int16x8ExtractLaneS(a,i*2)+Int16x8ExtractLaneS(a,i*2+1);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4ExtaddPairwiseI16x8U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var v=(uint)(Int16x8ExtractLaneU(a,i*2)+Int16x8ExtractLaneU(a,i*2+1));r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }

    // Q15MulrSat / Dot (polyfill)
    public static V128Polyfill Int16x8Q15MulrSatS(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<8;i++){var v=((int)Int16x8ExtractLaneS(a,i)*Int16x8ExtractLaneS(b,i)+0x4000)>>15;var s=v>32767?(short)32767:(short)v;r[i*2]=(byte)s;r[i*2+1]=(byte)((ushort)s>>8);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4DotI16x8S(V128Polyfill a, V128Polyfill b) { var r=new byte[16]; for(var i=0;i<4;i++){var v=Int16x8ExtractLaneS(a,i*2)*Int16x8ExtractLaneS(b,i*2)+Int16x8ExtractLaneS(a,i*2+1)*Int16x8ExtractLaneS(b,i*2+1);r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }

    // bitselect (polyfill)
    public static V128Polyfill V128Bitselect(V128Polyfill v1, V128Polyfill v2, V128Polyfill mask) => ApplyBinary(V128And(v1, mask), V128AndNot(v2, mask), (a, b) => (byte)(a | b));

    // trunc sat / convert / demote / promote (polyfill)
    public static V128Polyfill Int32x4TruncSatF32x4S(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var f=GetF32(a,i*4);int v=float.IsNaN(f)?0:f>=2147483647f?int.MaxValue:f<=-2147483648f?int.MinValue:(int)f;r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4TruncSatF32x4U(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<4;i++){var f=GetF32(a,i*4);uint v=float.IsNaN(f)||f<0?0u:f>=4294967295f?uint.MaxValue:(uint)f;r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4TruncSatF64x2SZero(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<2;i++){var f=i==0?GetF64Lo(a):GetF64Hi(a);int v=double.IsNaN(f)?0:f>=2147483647d?int.MaxValue:f<=-2147483648d?int.MinValue:(int)f;r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Int32x4TruncSatF64x2UZero(V128Polyfill a) { var r=new byte[16]; for(var i=0;i<2;i++){var f=i==0?GetF64Lo(a):GetF64Hi(a);uint v=double.IsNaN(f)||f<0?0u:f>=4294967295d?uint.MaxValue:(uint)f;r[i*4]=(byte)v;r[i*4+1]=(byte)(v>>8);r[i*4+2]=(byte)(v>>16);r[i*4+3]=(byte)(v>>24);} return Create(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],r[9],r[10],r[11],r[12],r[13],r[14],r[15]); }
    public static V128Polyfill Float32x4ConvertI32x4S(V128Polyfill a) { V128Polyfill r=default; for(var i=0;i<4;i++) r=SetF32(r,i*4,(float)Int32x4ExtractLane(a,i)); return r; }
    public static V128Polyfill Float32x4ConvertI32x4U(V128Polyfill a) { V128Polyfill r=default; for(var i=0;i<4;i++) r=SetF32(r,i*4,(float)(uint)Int32x4ExtractLane(a,i)); return r; }
    public static V128Polyfill Float64x2ConvertLowI32x4S(V128Polyfill a) { V128Polyfill r=default; r=SetF64(r,false,(double)Int32x4ExtractLane(a,0)); r=SetF64(r,true,(double)Int32x4ExtractLane(a,1)); return r; }
    public static V128Polyfill Float64x2ConvertLowI32x4U(V128Polyfill a) { V128Polyfill r=default; r=SetF64(r,false,(double)(uint)Int32x4ExtractLane(a,0)); r=SetF64(r,true,(double)(uint)Int32x4ExtractLane(a,1)); return r; }
    public static V128Polyfill Float32x4DemoteF64x2Zero(V128Polyfill a) { V128Polyfill r=default; r=SetF32(r,0,(float)GetF64Lo(a)); r=SetF32(r,4,(float)GetF64Hi(a)); return r; }
    public static V128Polyfill Float64x2PromoteLowF32x4(V128Polyfill a) { V128Polyfill r=default; r=SetF64(r,false,GetF32(a,0)); r=SetF64(r,true,GetF32(a,4)); return r; }
#pragma warning restore CS1591
#endif
}

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Runtime;

/// <summary>
/// Runtime helpers for SIMD v128 load/store/const operations.
/// On .NET 5+ these operate on <c>Vector128&lt;byte&gt;</c>;
/// on older runtimes they use <c>V128Polyfill</c>.
/// </summary>
public static class V128Helper
{
    internal static readonly RegeneratingWeakReference<MethodInfo> ReadUnalignedMethod = new(()
        => typeof(V128Helper).GetMethod(nameof(ReadUnaligned), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> WriteUnalignedMethod = new(()
        => typeof(V128Helper).GetMethod(nameof(WriteUnaligned), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CreateMethod = new(()
        => typeof(V128Helper).GetMethod(nameof(Create), BindingFlags.Public | BindingFlags.Static)!);

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

    // --- load/store lane ---
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load8LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load8Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load16LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load16Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load32LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load32Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load64LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load64Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Store8LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Store8Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Store16LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Store16Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Store32LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Store32Lane), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Store64LaneMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Store64Lane), BindingFlags.Public | BindingFlags.Static)!);

    // --- load zero ---
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load32ZeroMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load32Zero), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load64ZeroMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load64Zero), BindingFlags.Public | BindingFlags.Static)!);

    // --- extended loads (ptr → v128) ---
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load8x8SMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load8x8S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load8x8UMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load8x8U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load16x4SMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load16x4S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load16x4UMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load16x4U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load32x2SMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load32x2S), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load32x2UMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load32x2U), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load8SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load8Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load16SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load16Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load32SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load32Splat), BindingFlags.Public | BindingFlags.Static)!);
    internal static readonly RegeneratingWeakReference<MethodInfo> V128Load64SplatMethod = new(() => typeof(V128Helper).GetMethod(nameof(V128Load64Splat), BindingFlags.Public | BindingFlags.Static)!);

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
    internal static readonly RegeneratingWeakReference<MethodInfo> Int8x16ShuffleImmediateMethod = new(() => typeof(V128Helper).GetMethod(nameof(Int8x16ShuffleImmediate), BindingFlags.Public | BindingFlags.Static)!);
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

    /// <summary>The CLR type used to represent v128 at runtime on this platform.</summary>
    public static Type V128Type => typeof(System.Runtime.Intrinsics.Vector128<byte>);

    /// <summary>Read a 128-bit vector from an unaligned native pointer.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector128<byte> ReadUnaligned(IntPtr ptr)
        => Unsafe.ReadUnaligned<System.Runtime.Intrinsics.Vector128<byte>>((void*)ptr);

    /// <summary>Write a 128-bit vector to an unaligned native pointer.</summary>
    public static unsafe void WriteUnaligned(IntPtr ptr, System.Runtime.Intrinsics.Vector128<byte> value)
        => Unsafe.WriteUnaligned<System.Runtime.Intrinsics.Vector128<byte>>((void*)ptr, value);

    /// <summary>Create a v128 from 16 bytes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Create(
        byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7,
        byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14, byte b15)
        => Vector128.Create(b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15);


    /// <summary>i8x16 shuffle with precomputed source masks, avoiding per-call index array allocation.</summary>
    public static Vector128<byte> Int8x16ShuffleImmediate(
        Vector128<byte> a,
        Vector128<byte> b,
        Vector128<byte> maskA,
        Vector128<byte> maskB)
    {
        if (Ssse3.IsSupported)
            return Sse2.Or(Ssse3.Shuffle(a, maskA), Ssse3.Shuffle(b, maskB));

        Span<byte> result = stackalloc byte[16];
        for (var i = 0; i < 16; i++)
        {
            byte lane = 0;
            var selectA = maskA.GetElement(i);
            if ((selectA & 0x80) == 0)
                lane = a.GetElement(selectA);

            var selectB = maskB.GetElement(i);
            if ((selectB & 0x80) == 0)
                lane = b.GetElement(selectB);

            result[i] = lane;
        }

        return Vector128.Create(
            result[0], result[1], result[2], result[3],
            result[4], result[5], result[6], result[7],
            result[8], result[9], result[10], result[11],
            result[12], result[13], result[14], result[15]);
    }
    /// <summary>i8x16 swizzle (select lanes of a by indices in b, 0-15; out-of-range → 0).</summary>
    public static Vector128<byte> Int8x16Swizzle(Vector128<byte> a, Vector128<byte> b)
    {
        if (Ssse3.IsSupported)
        {
            var highBit = Vector128.Create((byte)0x80);
            var compareBase = Vector128.Create(unchecked((sbyte)(0x80 | 0x0F)));
            var invalid = Sse2.CompareGreaterThan(Sse2.Xor(b, highBit).AsSByte(), compareBase);
            var mask = Sse2.Or(
                Sse2.AndNot(invalid.AsByte(), b),
                Sse2.And(invalid.AsByte(), highBit));
            return Ssse3.Shuffle(a, mask);
        }

        Span<byte> result = stackalloc byte[16];
        for (var i = 0; i < 16; i++)
        {
            var idx = b.GetElement(i);
            result[i] = idx < 16 ? a.GetElement(idx) : (byte)0;
        }

        return Vector128.Create(
            result[0], result[1], result[2], result[3],
            result[4], result[5], result[6], result[7],
            result[8], result[9], result[10], result[11],
            result[12], result[13], result[14], result[15]);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Int32x4Add(Vector128<byte> a, Vector128<byte> b) => (a.AsInt32() + b.AsInt32()).AsByte();
    /// <summary>i32x4 subtract.</summary>
    public static Vector128<byte> Int32x4Sub(Vector128<byte> a, Vector128<byte> b) => (a.AsInt32() - b.AsInt32()).AsByte();
    /// <summary>i32x4 multiply.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    /// <summary>f32x4 IEEE min (propagates NaN, returns -0 over +0).</summary>
    public static Vector128<byte> Float32x4Min(Vector128<byte> a, Vector128<byte> b)
    {
#if NET9_0_OR_GREATER
        return Vector128.Min(a.AsSingle(), b.AsSingle()).AsByte();
#else
        var r = new float[4];
        var sa = a.AsSingle(); var sb = b.AsSingle();
        for (var i = 0; i < 4; i++)
        {
            var ai = sa.GetElement(i); var bi = sb.GetElement(i);
            float ri;
            if (float.IsNaN(ai) || float.IsNaN(bi)) ri = float.NaN;
            else if (ai == 0 && bi == 0) ri = FloatHelper.UInt32BitsToFloat(FloatHelper.FloatToUInt32Bits(ai) | FloatHelper.FloatToUInt32Bits(bi));
            else ri = ai < bi ? ai : bi;
            r[i] = ri;
        }
        return Vector128.Create(r).AsByte();
#endif
    }
    /// <summary>f32x4 IEEE max (propagates NaN, returns +0 over -0).</summary>
    public static Vector128<byte> Float32x4Max(Vector128<byte> a, Vector128<byte> b)
    {
#if NET9_0_OR_GREATER
        return Vector128.Max(a.AsSingle(), b.AsSingle()).AsByte();
#else
        var r = new float[4];
        var sa = a.AsSingle(); var sb = b.AsSingle();
        for (var i = 0; i < 4; i++)
        {
            var ai = sa.GetElement(i); var bi = sb.GetElement(i);
            float ri;
            if (float.IsNaN(ai) || float.IsNaN(bi)) ri = float.NaN;
            else if (ai == 0 && bi == 0) ri = FloatHelper.UInt32BitsToFloat(FloatHelper.FloatToUInt32Bits(ai) & FloatHelper.FloatToUInt32Bits(bi));
            else ri = ai > bi ? ai : bi;
            r[i] = ri;
        }
        return Vector128.Create(r).AsByte();
#endif
    }
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
    /// <summary>f64x2 IEEE min (propagates NaN, returns -0 over +0).</summary>
    public static Vector128<byte> Float64x2Min(Vector128<byte> a, Vector128<byte> b)
    {
#if NET9_0_OR_GREATER
        return Vector128.Min(a.AsDouble(), b.AsDouble()).AsByte();
#else
        var r = new double[2];
        var sa = a.AsDouble(); var sb = b.AsDouble();
        for (var i = 0; i < 2; i++)
        {
            var ai = sa.GetElement(i); var bi = sb.GetElement(i);
            double ri;
            if (double.IsNaN(ai) || double.IsNaN(bi)) ri = double.NaN;
            else if (ai == 0 && bi == 0) ri = FloatHelper.UInt64BitsToDouble(FloatHelper.DoubleToUInt64Bits(ai) | FloatHelper.DoubleToUInt64Bits(bi));
            else ri = ai < bi ? ai : bi;
            r[i] = ri;
        }
        return Vector128.Create(r).AsByte();
#endif
    }
    /// <summary>f64x2 IEEE max (propagates NaN, returns +0 over -0).</summary>
    public static Vector128<byte> Float64x2Max(Vector128<byte> a, Vector128<byte> b)
    {
#if NET9_0_OR_GREATER
        return Vector128.Max(a.AsDouble(), b.AsDouble()).AsByte();
#else
        var r = new double[2];
        var sa = a.AsDouble(); var sb = b.AsDouble();
        for (var i = 0; i < 2; i++)
        {
            var ai = sa.GetElement(i); var bi = sb.GetElement(i);
            double ri;
            if (double.IsNaN(ai) || double.IsNaN(bi)) ri = double.NaN;
            else if (ai == 0 && bi == 0) ri = FloatHelper.UInt64BitsToDouble(FloatHelper.DoubleToUInt64Bits(ai) & FloatHelper.DoubleToUInt64Bits(bi));
            else ri = ai > bi ? ai : bi;
            r[i] = ri;
        }
        return Vector128.Create(r).AsByte();
#endif
    }
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
    public static Vector128<byte> Int8x16NarrowI16x8S(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.PackSignedSaturate(a.AsInt16(), b.AsInt16()).AsByte();

        Span<sbyte> r = stackalloc sbyte[16];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i); r[i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        for (var i = 0; i < 8; i++) { var v = b.AsInt16().GetElement(i); r[8 + i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        return Vector128.Create(
            r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7],
            r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15]).AsByte();
    }
    public static Vector128<byte> Int8x16NarrowI16x8U(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.PackUnsignedSaturate(a.AsInt16(), b.AsInt16());

        Span<byte> r = stackalloc byte[16];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i); r[i] = v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v; }
        for (var i = 0; i < 8; i++) { var v = b.AsInt16().GetElement(i); r[8 + i] = v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v; }
        return Vector128.Create(
            r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7],
            r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15]);
    }
    public static Vector128<byte> Int16x8NarrowI32x4S(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.PackSignedSaturate(a.AsInt32(), b.AsInt32()).AsByte();

        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 4; i++) { var v = a.AsInt32().GetElement(i); r[i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        for (var i = 0; i < 4; i++) { var v = b.AsInt32().GetElement(i); r[4 + i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    public static Vector128<byte> Int16x8NarrowI32x4U(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse41.IsSupported)
            return Sse41.PackUnsignedSaturate(a.AsInt32(), b.AsInt32()).AsByte();

        Span<ushort> r = stackalloc ushort[8];
        for (var i = 0; i < 4; i++) { var v = a.AsInt32().GetElement(i); r[i] = v < 0 ? (ushort)0 : v > 65535 ? (ushort)65535 : (ushort)v; }
        for (var i = 0; i < 4; i++) { var v = b.AsInt32().GetElement(i); r[4 + i] = v < 0 ? (ushort)0 : v > 65535 ? (ushort)65535 : (ushort)v; }
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }

    // --- extend (NET5+) ---
    public static Vector128<byte> Int16x8ExtLowI8x16S(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var sign = Sse2.CompareGreaterThan(Vector128<sbyte>.Zero, a.AsSByte()).AsByte();
            return Sse2.UnpackLow(a, sign).AsByte();
        }

        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 8; i++) r[i] = (sbyte)a.GetElement(i);
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    public static Vector128<byte> Int16x8ExtHighI8x16S(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var sign = Sse2.CompareGreaterThan(Vector128<sbyte>.Zero, a.AsSByte()).AsByte();
            return Sse2.UnpackHigh(a, sign).AsByte();
        }

        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 8; i++) r[i] = (sbyte)a.GetElement(8 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Int16x8ExtLowI8x16U(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackLow(a, Vector128<byte>.Zero).AsByte();

        Span<ushort> r = stackalloc ushort[8];
        for (var i = 0; i < 8; i++) r[i] = a.GetElement(i);
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    public static Vector128<byte> Int16x8ExtHighI8x16U(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a, Vector128<byte>.Zero).AsByte();

        Span<ushort> r = stackalloc ushort[8];
        for (var i = 0; i < 8; i++) r[i] = a.GetElement(8 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    public static Vector128<byte> Int32x4ExtLowI16x8S(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var lanes = a.AsInt16();
            var sign = Sse2.CompareGreaterThan(Vector128<short>.Zero, lanes);
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        Span<int> r = stackalloc int[4];
        for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(i);
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
    public static Vector128<byte> Int32x4ExtHighI16x8S(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var lanes = a.AsInt16();
            var sign = Sse2.CompareGreaterThan(Vector128<short>.Zero, lanes);
            return Sse2.UnpackHigh(lanes, sign).AsByte();
        }

        Span<int> r = stackalloc int[4];
        for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(4 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Int32x4ExtLowI16x8U(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackLow(a.AsUInt16(), Vector128<ushort>.Zero).AsByte();

        Span<uint> r = stackalloc uint[4];
        for (var i = 0; i < 4; i++) r[i] = a.AsUInt16().GetElement(i);
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
    public static Vector128<byte> Int32x4ExtHighI16x8U(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a.AsUInt16(), Vector128<ushort>.Zero).AsByte();

        Span<uint> r = stackalloc uint[4];
        for (var i = 0; i < 4; i++) r[i] = a.AsUInt16().GetElement(4 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
    public static Vector128<byte> Int64x2ExtLowI32x4S(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var lanes = a.AsInt32();
            var sign = Sse2.CompareGreaterThan(Vector128<int>.Zero, lanes);
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        Span<long> r = stackalloc long[2];
        for (var i = 0; i < 2; i++) r[i] = a.AsInt32().GetElement(i);
        return Vector128.Create(r[0], r[1]).AsByte();
    }
    public static Vector128<byte> Int64x2ExtHighI32x4S(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var lanes = a.AsInt32();
            var sign = Sse2.CompareGreaterThan(Vector128<int>.Zero, lanes);
            return Sse2.UnpackHigh(lanes, sign).AsByte();
        }

        Span<long> r = stackalloc long[2];
        for (var i = 0; i < 2; i++) r[i] = a.AsInt32().GetElement(2 + i);
        return Vector128.Create(r[0], r[1]).AsByte();
    }
    public static Vector128<byte> Int64x2ExtLowI32x4U(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackLow(a.AsUInt32(), Vector128<uint>.Zero).AsByte();

        Span<ulong> r = stackalloc ulong[2];
        for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(i);
        return Vector128.Create(r[0], r[1]).AsByte();
    }
    public static Vector128<byte> Int64x2ExtHighI32x4U(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a.AsUInt32(), Vector128<uint>.Zero).AsByte();

        Span<ulong> r = stackalloc ulong[2];
        for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(2 + i);
        return Vector128.Create(r[0], r[1]).AsByte();
    }

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

    // --- load/store lane (NET5+) ---
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<byte> V128Load8Lane(IntPtr ptr, Vector128<byte> vec, int lane) => vec.WithElement(lane, *(byte*)ptr);
    public static unsafe Vector128<byte> V128Load16Lane(IntPtr ptr, Vector128<byte> vec, int lane) { var p=(byte*)ptr; return vec.AsInt16().WithElement(lane,(short)(p[0]|(p[1]<<8))).AsByte(); }
    public static unsafe Vector128<byte> V128Load32Lane(IntPtr ptr, Vector128<byte> vec, int lane) { var p=(byte*)ptr; return vec.AsInt32().WithElement(lane,p[0]|(p[1]<<8)|(p[2]<<16)|(p[3]<<24)).AsByte(); }
    public static unsafe Vector128<byte> V128Load64Lane(IntPtr ptr, Vector128<byte> vec, int lane) { var p=(byte*)ptr; return vec.AsInt64().WithElement(lane,(long)((ulong)p[0]|((ulong)p[1]<<8)|((ulong)p[2]<<16)|((ulong)p[3]<<24)|((ulong)p[4]<<32)|((ulong)p[5]<<40)|((ulong)p[6]<<48)|((ulong)p[7]<<56))).AsByte(); }
    public static unsafe void V128Store8Lane(IntPtr ptr, Vector128<byte> vec, int lane) => *(byte*)ptr = vec.GetElement(lane);
    public static unsafe void V128Store16Lane(IntPtr ptr, Vector128<byte> vec, int lane) { var v=(ushort)(ushort)vec.AsInt16().GetElement(lane); var p=(byte*)ptr; p[0]=(byte)v; p[1]=(byte)(v>>8); }
    public static unsafe void V128Store32Lane(IntPtr ptr, Vector128<byte> vec, int lane) { var v=(uint)vec.AsInt32().GetElement(lane); var p=(byte*)ptr; p[0]=(byte)v; p[1]=(byte)(v>>8); p[2]=(byte)(v>>16); p[3]=(byte)(v>>24); }
    public static unsafe void V128Store64Lane(IntPtr ptr, Vector128<byte> vec, int lane) { var v=(ulong)vec.AsInt64().GetElement(lane); var p=(byte*)ptr; p[0]=(byte)v; p[1]=(byte)(v>>8); p[2]=(byte)(v>>16); p[3]=(byte)(v>>24); p[4]=(byte)(v>>32); p[5]=(byte)(v>>40); p[6]=(byte)(v>>48); p[7]=(byte)(v>>56); }

    // --- load zero (NET5+) ---
    public static unsafe Vector128<byte> V128Load32Zero(IntPtr ptr) { var p=(byte*)ptr; return Vector128.Create(p[0]|(p[1]<<8)|(p[2]<<16)|(p[3]<<24),0,0,0).AsByte(); }
    public static unsafe Vector128<byte> V128Load64Zero(IntPtr ptr) { var p=(byte*)ptr; return Vector128.Create((long)((ulong)p[0]|((ulong)p[1]<<8)|((ulong)p[2]<<16)|((ulong)p[3]<<24)|((ulong)p[4]<<32)|((ulong)p[5]<<40)|((ulong)p[6]<<48)|((ulong)p[7]<<56)),0L).AsByte(); }

    // --- extended loads (NET5+) ---
    public static unsafe Vector128<byte> V128Load8x8S(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsByte();
            var sign = Sse2.CompareGreaterThan(Vector128<sbyte>.Zero, lanes.AsSByte()).AsByte();
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        var p = (byte*)ptr;
        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 8; i++) r[i] = (sbyte)p[i];
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    public static unsafe Vector128<byte> V128Load8x8U(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsByte();
            return Sse2.UnpackLow(lanes, Vector128<byte>.Zero).AsByte();
        }

        var p = (byte*)ptr;
        Span<ushort> r = stackalloc ushort[8];
        for (var i = 0; i < 8; i++) r[i] = p[i];
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
    public static unsafe Vector128<byte> V128Load16x4S(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsInt16();
            var sign = Sse2.CompareGreaterThan(Vector128<short>.Zero, lanes);
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        var p = (byte*)ptr;
        Span<int> r = stackalloc int[4];
        for (var i = 0; i < 4; i++) r[i] = (short)(p[i * 2] | (p[i * 2 + 1] << 8));
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
    public static unsafe Vector128<byte> V128Load16x4U(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsUInt16();
            return Sse2.UnpackLow(lanes, Vector128<ushort>.Zero).AsByte();
        }

        var p = (byte*)ptr;
        Span<uint> r = stackalloc uint[4];
        for (var i = 0; i < 4; i++) r[i] = (ushort)(p[i * 2] | (p[i * 2 + 1] << 8));
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
    public static unsafe Vector128<byte> V128Load32x2S(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsInt32();
            var sign = Sse2.CompareGreaterThan(Vector128<int>.Zero, lanes);
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        var p = (byte*)ptr;
        Span<long> r = stackalloc long[2];
        for (var i = 0; i < 2; i++) r[i] = (int)(p[i * 4] | (p[i * 4 + 1] << 8) | (p[i * 4 + 2] << 16) | (p[i * 4 + 3] << 24));
        return Vector128.Create(r[0], r[1]).AsByte();
    }
    public static unsafe Vector128<byte> V128Load32x2U(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsUInt32();
            return Sse2.UnpackLow(lanes, Vector128<uint>.Zero).AsByte();
        }

        var p = (byte*)ptr;
        Span<ulong> r = stackalloc ulong[2];
        for (var i = 0; i < 2; i++) r[i] = (uint)(p[i * 4] | (p[i * 4 + 1] << 8) | (p[i * 4 + 2] << 16) | (p[i * 4 + 3] << 24));
        return Vector128.Create(r[0], r[1]).AsByte();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<byte> V128Load8Splat(IntPtr ptr) => Vector128.Create(*((byte*)ptr));
    public static unsafe Vector128<byte> V128Load16Splat(IntPtr ptr) { var p = (byte*)ptr; return Vector128.Create((short)(p[0]|(p[1]<<8))).AsByte(); }
    public static unsafe Vector128<byte> V128Load32Splat(IntPtr ptr) { var p = (byte*)ptr; return Vector128.Create(p[0]|(p[1]<<8)|(p[2]<<16)|(p[3]<<24)).AsByte(); }
    public static unsafe Vector128<byte> V128Load64Splat(IntPtr ptr) { var p = (byte*)ptr; return Vector128.Create((long)((ulong)p[0]|((ulong)p[1]<<8)|((ulong)p[2]<<16)|((ulong)p[3]<<24)|((ulong)p[4]<<32)|((ulong)p[5]<<40)|((ulong)p[6]<<48)|((ulong)p[7]<<56))).AsByte(); }
#pragma warning restore CS1591
}

using System.Reflection;
using System.Runtime.CompilerServices;

namespace WebAssembly.Runtime;

/// <summary>
/// Runtime helpers for bit-exact float operations that bypass CLR NaN canonicalization.
/// </summary>
public static class FloatHelper
{
    internal static readonly RegeneratingWeakReference<MethodInfo> UInt32BitsToFloatMethod = new(
        () => typeof(FloatHelper).GetMethod(nameof(UInt32BitsToFloat), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> UInt64BitsToDoubleMethod = new(
        () => typeof(FloatHelper).GetMethod(nameof(UInt64BitsToDouble), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> FloatToUInt32BitsMethod = new(
        () => typeof(FloatHelper).GetMethod(nameof(FloatToUInt32Bits), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> DoubleToUInt64BitsMethod = new(
        () => typeof(FloatHelper).GetMethod(nameof(DoubleToUInt64Bits), BindingFlags.Public | BindingFlags.Static)!);

    /// <summary>Reinterprets the bit pattern of a <see cref="uint"/> as a <see cref="float"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float UInt32BitsToFloat(uint bits) => *(float*)&bits;

    /// <summary>Reinterprets the bit pattern of a <see cref="ulong"/> as a <see cref="double"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double UInt64BitsToDouble(ulong bits) => *(double*)&bits;

    /// <summary>Reinterprets the bit pattern of a <see cref="float"/> as a <see cref="uint"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint FloatToUInt32Bits(float value) => *(uint*)&value;

    /// <summary>Reinterprets the bit pattern of a <see cref="double"/> as a <see cref="ulong"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong DoubleToUInt64Bits(double value) => *(ulong*)&value;

    internal static readonly RegeneratingWeakReference<MethodInfo> CanonicalizeFloat32Method = new(
        () => typeof(FloatHelper).GetMethod(nameof(CanonicalizeFloat32), BindingFlags.Public | BindingFlags.Static)!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CanonicalizeFloat64Method = new(
        () => typeof(FloatHelper).GetMethod(nameof(CanonicalizeFloat64), BindingFlags.Public | BindingFlags.Static)!);

    // Canonical qNaN bit patterns per WASM spec.
    private const uint CanonicalFloat32NaN = 0x7FC00000u;
    private const ulong CanonicalFloat64NaN = 0x7FF8000000000000UL;

    /// <summary>Returns the canonical qNaN if <paramref name="value"/> is NaN; otherwise returns <paramref name="value"/> unchanged.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CanonicalizeFloat32(float value) => float.IsNaN(value) ? UInt32BitsToFloat(CanonicalFloat32NaN) : value;

    /// <summary>Returns the canonical qNaN if <paramref name="value"/> is NaN; otherwise returns <paramref name="value"/> unchanged.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CanonicalizeFloat64(double value) => double.IsNaN(value) ? UInt64BitsToDouble(CanonicalFloat64NaN) : value;
}

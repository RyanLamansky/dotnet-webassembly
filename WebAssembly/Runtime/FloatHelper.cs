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
    public static float UInt32BitsToFloat(uint bits)
    {
        var b = bits;
        return Unsafe.As<uint, float>(ref b);
    }

    /// <summary>Reinterprets the bit pattern of a <see cref="ulong"/> as a <see cref="double"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double UInt64BitsToDouble(ulong bits)
    {
        var b = bits;
        return Unsafe.As<ulong, double>(ref b);
    }

    /// <summary>Reinterprets the bit pattern of a <see cref="float"/> as a <see cref="uint"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FloatToUInt32Bits(float value) => Unsafe.As<float, uint>(ref value);

    /// <summary>Reinterprets the bit pattern of a <see cref="double"/> as a <see cref="ulong"/>, preserving NaN payloads.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong DoubleToUInt64Bits(double value) => Unsafe.As<double, ulong>(ref value);
}

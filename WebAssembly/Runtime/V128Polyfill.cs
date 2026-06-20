#if !NET5_0_OR_GREATER
using System.Runtime.InteropServices;

namespace WebAssembly.Runtime;

/// <summary>
/// A 16-byte value type used to represent WASM v128 on runtimes that do not provide
/// <c>System.Runtime.Intrinsics.Vector128&lt;byte&gt;</c> (.NET older than 5).
/// On .NET 5+, <c>Vector128&lt;byte&gt;</c> is used instead and this type does not exist.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct V128Polyfill
{
#pragma warning disable CS1591
    public byte B0, B1, B2, B3, B4, B5, B6, B7, B8, B9, B10, B11, B12, B13, B14, B15;
#pragma warning restore CS1591

    /// <summary>Bitwise AND of two values.</summary>
    public static unsafe V128Polyfill operator &(V128Polyfill left, V128Polyfill right)
    {
        var result = default(V128Polyfill);
        ulong* l = (ulong*)&left, r = (ulong*)&right, o = (ulong*)&result;
        o[0] = l[0] & r[0];
        o[1] = l[1] & r[1];
        return result;
    }

    /// <summary>Bitwise OR of two values.</summary>
    public static unsafe V128Polyfill operator |(V128Polyfill left, V128Polyfill right)
    {
        var result = default(V128Polyfill);
        ulong* l = (ulong*)&left, r = (ulong*)&right, o = (ulong*)&result;
        o[0] = l[0] | r[0];
        o[1] = l[1] | r[1];
        return result;
    }

    /// <summary>Bitwise XOR of two values.</summary>
    public static unsafe V128Polyfill operator ^(V128Polyfill left, V128Polyfill right)
    {
        var result = default(V128Polyfill);
        ulong* l = (ulong*)&left, r = (ulong*)&right, o = (ulong*)&result;
        o[0] = l[0] ^ r[0];
        o[1] = l[1] ^ r[1];
        return result;
    }

    /// <summary>Bitwise complement (one's complement) of a value.</summary>
    public static unsafe V128Polyfill operator ~(V128Polyfill value)
    {
        var result = default(V128Polyfill);
        ulong* v = (ulong*)&value, o = (ulong*)&result;
        o[0] = ~v[0];
        o[1] = ~v[1];
        return result;
    }

    /// <summary>Bitwise AND of <paramref name="left"/> with the complement of <paramref name="right"/> (<c>left &amp; ~right</c>).</summary>
    public static unsafe V128Polyfill AndNot(V128Polyfill left, V128Polyfill right)
    {
        var result = default(V128Polyfill);
        ulong* l = (ulong*)&left, r = (ulong*)&right, o = (ulong*)&result;
        o[0] = l[0] & ~r[0];
        o[1] = l[1] & ~r[1];
        return result;
    }
}
#endif

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
}
#endif

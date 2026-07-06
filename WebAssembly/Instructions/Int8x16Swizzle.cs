using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Swizzle i8x16 lanes according to an index vector.</summary>
public class Int8x16Swizzle : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Swizzle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Swizzle;

    /// <summary>Creates a new <see cref="Int8x16Swizzle"/> instance.</summary>
    public Int8x16Swizzle() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) =>
        // Vector128.Shuffle zeroes any lane whose index is out of range (>= 16),
        // which matches i8x16.swizzle semantics exactly.
        Vector128.Shuffle(a, b);
}

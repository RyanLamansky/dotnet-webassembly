using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i8x16 unsigned less-than-or-equal.</summary>
public class Int8x16LessThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanOrEqualUnsigned;

    /// <summary>Creates a new <see cref="Int8x16LessThanOrEqualUnsigned"/> instance.</summary>
    public Int8x16LessThanOrEqualUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a, b);
}

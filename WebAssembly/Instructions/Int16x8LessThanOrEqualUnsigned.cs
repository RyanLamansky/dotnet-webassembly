using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 unsigned less-than-or-equal.</summary>
public class Int16x8LessThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanOrEqualUnsigned;

    /// <summary>Creates a new <see cref="Int16x8LessThanOrEqualUnsigned"/> instance.</summary>
    public Int16x8LessThanOrEqualUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsUInt16(), b.AsUInt16()).AsByte();
}

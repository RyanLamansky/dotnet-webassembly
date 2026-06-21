using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 unsigned greater-than-or-equal.</summary>
public class Int16x8GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanOrEqualUnsigned;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int16x8GreaterThanOrEqualUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsUInt16(), b.AsUInt16()).AsByte();
}

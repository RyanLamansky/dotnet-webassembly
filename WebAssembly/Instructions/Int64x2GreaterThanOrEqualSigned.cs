using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i64x2 signed greater-than-or-equal.</summary>
public class Int64x2GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2GreaterThanOrEqualSigned;

    /// <summary>Creates a new <see cref="Int64x2GreaterThanOrEqualSigned"/> instance.</summary>
    public Int64x2GreaterThanOrEqualSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsInt64(), b.AsInt64()).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2LessThanOrEqualSigned instruction.</summary>
public class Int64x2LessThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2LessThanOrEqualSigned;

    /// <summary>Creates a new <see cref="Int64x2LessThanOrEqualSigned"/> instance.</summary>
    public Int64x2LessThanOrEqualSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsInt64(), b.AsInt64()).AsByte();
}

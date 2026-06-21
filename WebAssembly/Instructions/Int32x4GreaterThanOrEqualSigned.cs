using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i32x4 signed greater-than-or-equal.</summary>
public class Int32x4GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanOrEqualSigned;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanOrEqualSigned"/> instance.</summary>
    public Int32x4GreaterThanOrEqualSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsInt32(), b.AsInt32()).AsByte();
}

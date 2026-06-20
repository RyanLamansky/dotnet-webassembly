using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanOrEqualSigned instruction.</summary>
public class Int32x4LessThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanOrEqualSigned;

    /// <summary>Creates a new <see cref="Int32x4LessThanOrEqualSigned"/> instance.</summary>
    public Int32x4LessThanOrEqualSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsInt32(), b.AsInt32()).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Neg instruction.</summary>
public class Int64x2Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Neg;

    /// <summary>Creates a new <see cref="Int64x2Neg"/> instance.</summary>
    public Int64x2Neg() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => (-a.AsInt64()).AsByte();
}

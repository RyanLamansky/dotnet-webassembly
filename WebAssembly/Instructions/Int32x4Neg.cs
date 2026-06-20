using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Neg instruction.</summary>
public class Int32x4Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Neg;

    /// <summary>Creates a new <see cref="Int32x4Neg"/> instance.</summary>
    public Int32x4Neg() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => (-a.AsInt32()).AsByte();
}

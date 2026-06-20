using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Neg instruction.</summary>
public class Int8x16Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Neg;

    /// <summary>Creates a new <see cref="Int8x16Neg"/> instance.</summary>
    public Int8x16Neg() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => (-a.AsSByte()).AsByte();
}

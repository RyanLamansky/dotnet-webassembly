using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 negate.</summary>
public class Int16x8Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Neg;

    /// <summary>Creates a new <see cref="Int16x8Neg"/> instance.</summary>
    public Int16x8Neg() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => (-a.AsInt16()).AsByte();
}

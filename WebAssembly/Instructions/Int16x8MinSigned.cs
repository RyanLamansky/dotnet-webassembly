using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 signed min.</summary>
public class Int16x8MinSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MinSigned;

    /// <summary>Creates a new <see cref="Int16x8MinSigned"/> instance.</summary>
    public Int16x8MinSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsInt16(), b.AsInt16()).AsByte();
}

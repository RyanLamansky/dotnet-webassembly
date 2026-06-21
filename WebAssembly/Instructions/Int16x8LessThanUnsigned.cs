using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 unsigned less-than.</summary>
public class Int16x8LessThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanUnsigned;

    /// <summary>Creates a new <see cref="Int16x8LessThanUnsigned"/> instance.</summary>
    public Int16x8LessThanUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsUInt16(), b.AsUInt16()).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 unsigned greater-than.</summary>
public class Int16x8GreaterThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanUnsigned;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanUnsigned"/> instance.</summary>
    public Int16x8GreaterThanUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsUInt16(), b.AsUInt16()).AsByte();
}

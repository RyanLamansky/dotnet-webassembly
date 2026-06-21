using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i8x16 unsigned max.</summary>
public class Int8x16MaxUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MaxUnsigned;

    /// <summary>Creates a new <see cref="Int8x16MaxUnsigned"/> instance.</summary>
    public Int8x16MaxUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a, b);
}

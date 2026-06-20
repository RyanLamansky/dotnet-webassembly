using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MinUnsigned instruction.</summary>
public class Int8x16MinUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MinUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MinUnsigned;

    /// <summary>Creates a new <see cref="Int8x16MinUnsigned"/> instance.</summary>
    public Int8x16MinUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a, b);
}

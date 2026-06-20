using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16GreaterThanOrEqualUnsigned instruction.</summary>
public class Int8x16GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16GreaterThanOrEqualUnsigned;

    /// <summary>Creates a new <see cref="Int8x16GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int8x16GreaterThanOrEqualUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a, b);
}

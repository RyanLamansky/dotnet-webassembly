using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i8x16 not equal.</summary>
public class Int8x16NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NotEqual;

    /// <summary>Creates a new <see cref="Int8x16NotEqual"/> instance.</summary>
    public Int8x16NotEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => ~Vector128.Equals(a, b);
}

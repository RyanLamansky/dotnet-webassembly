using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i64x2 not equal.</summary>
public class Int64x2NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2NotEqual;

    /// <summary>Creates a new <see cref="Int64x2NotEqual"/> instance.</summary>
    public Int64x2NotEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsInt64(), b.AsInt64())).AsByte();
}

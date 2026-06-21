using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i64x2 subtract.</summary>
public class Int64x2Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Sub;

    /// <summary>Creates a new <see cref="Int64x2Sub"/> instance.</summary>
    public Int64x2Sub() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsInt64() - b.AsInt64()).AsByte();
}

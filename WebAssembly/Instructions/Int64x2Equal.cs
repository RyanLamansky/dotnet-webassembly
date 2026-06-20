using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Equal instruction.</summary>
public class Int64x2Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Equal;

    /// <summary>Creates a new <see cref="Int64x2Equal"/> instance.</summary>
    public Int64x2Equal() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsInt64(), b.AsInt64()).AsByte();
}

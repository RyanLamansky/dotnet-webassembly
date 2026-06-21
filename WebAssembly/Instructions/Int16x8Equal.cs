using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 equal.</summary>
public class Int16x8Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Equal;

    /// <summary>Creates a new <see cref="Int16x8Equal"/> instance.</summary>
    public Int16x8Equal() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a.AsInt16(), b.AsInt16()).AsByte();
}

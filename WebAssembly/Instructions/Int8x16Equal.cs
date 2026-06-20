using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Equal instruction.</summary>
public class Int8x16Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Equal;

    /// <summary>Creates a new <see cref="Int8x16Equal"/> instance.</summary>
    public Int8x16Equal() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Equals(a, b);
}

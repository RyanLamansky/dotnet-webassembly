using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MaxSigned instruction.</summary>
public class Int8x16MaxSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MaxSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MaxSigned;

    /// <summary>Creates a new <see cref="Int8x16MaxSigned"/> instance.</summary>
    public Int8x16MaxSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsSByte(), b.AsSByte()).AsByte();
}

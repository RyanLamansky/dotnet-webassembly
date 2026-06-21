using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i8x16 signed greater-than.</summary>
public class Int8x16GreaterThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16GreaterThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16GreaterThanSigned;

    /// <summary>Creates a new <see cref="Int8x16GreaterThanSigned"/> instance.</summary>
    public Int8x16GreaterThanSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsSByte(), b.AsSByte()).AsByte();
}

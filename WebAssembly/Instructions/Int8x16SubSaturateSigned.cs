using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i8x16 signed subtract with saturation.</summary>
public class Int8x16SubSaturateSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16SubSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16SubSaturateSigned;

    /// <summary>Creates a new <see cref="Int8x16SubSaturateSigned"/> instance.</summary>
    public Int8x16SubSaturateSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new sbyte[16];
        for (var i = 0; i < 16; i++) { var v = a.AsSByte().GetElement(i) - b.AsSByte().GetElement(i); r[i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        return Vector128.Create(r).AsByte();
    }
}

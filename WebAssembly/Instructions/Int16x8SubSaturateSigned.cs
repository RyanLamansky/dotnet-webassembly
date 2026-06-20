using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8SubSaturateSigned instruction.</summary>
public class Int16x8SubSaturateSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8SubSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8SubSaturateSigned;

    /// <summary>Creates a new <see cref="Int16x8SubSaturateSigned"/> instance.</summary>
    public Int16x8SubSaturateSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new short[8];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i) - b.AsInt16().GetElement(i); r[i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        return Vector128.Create(r).AsByte();
    }
}

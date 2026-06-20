using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Q15MulrSatSigned instruction.</summary>
public class Int16x8Q15MulrSatSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Q15MulrSatSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Q15MulrSatSigned;

    /// <summary>Creates a new <see cref="Int16x8Q15MulrSatSigned"/> instance.</summary>
    public Int16x8Q15MulrSatSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new short[8]; for (var i = 0; i < 8; i++) { var v = ((int)a.AsInt16().GetElement(i) * b.AsInt16().GetElement(i) + 0x4000) >> 15; r[i] = v > 32767 ? (short)32767 : (short)v; } return Vector128.Create(r).AsByte(); }
}

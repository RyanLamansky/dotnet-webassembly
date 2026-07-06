using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed i16x8 Q15 multiply; the deterministic profile is identical to <see cref="Int16x8Q15MulrSatSigned"/>.</summary>
public class Int16x8RelaxedQ15MulrSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8RelaxedQ15MulrSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8RelaxedQ15MulrSigned;

    /// <summary>Creates a new <see cref="Int16x8RelaxedQ15MulrSigned"/> instance.</summary>
    public Int16x8RelaxedQ15MulrSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Int16x8Q15MulrSatSigned.Execute(a, b);
}

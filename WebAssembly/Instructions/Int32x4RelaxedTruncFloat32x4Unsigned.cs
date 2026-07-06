using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed truncate f32x4 to i32x4, unsigned; the deterministic profile is identical to <see cref="Int32x4TruncSatFloat32x4Unsigned"/>.</summary>
public class Int32x4RelaxedTruncFloat32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4RelaxedTruncFloat32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4RelaxedTruncFloat32x4Unsigned;

    /// <summary>Creates a new <see cref="Int32x4RelaxedTruncFloat32x4Unsigned"/> instance.</summary>
    public Int32x4RelaxedTruncFloat32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Int32x4TruncSatFloat32x4Unsigned.Execute(a);
}

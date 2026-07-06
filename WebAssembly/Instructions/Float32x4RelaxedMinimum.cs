using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed f32x4 min; the deterministic profile is identical to <see cref="Float32x4Min"/>.</summary>
public class Float32x4RelaxedMinimum : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4RelaxedMinimum"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4RelaxedMinimum;

    /// <summary>Creates a new <see cref="Float32x4RelaxedMinimum"/> instance.</summary>
    public Float32x4RelaxedMinimum() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Float32x4Min.Execute(a, b);
}

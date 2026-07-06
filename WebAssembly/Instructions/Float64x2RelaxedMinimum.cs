using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed f64x2 min; the deterministic profile is identical to <see cref="Float64x2Min"/>.</summary>
public class Float64x2RelaxedMinimum : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2RelaxedMinimum"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2RelaxedMinimum;

    /// <summary>Creates a new <see cref="Float64x2RelaxedMinimum"/> instance.</summary>
    public Float64x2RelaxedMinimum() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Float64x2Min.Execute(a, b);
}

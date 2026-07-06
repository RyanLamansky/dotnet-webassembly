using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed f64x2 min; the deterministic profile is identical to <see cref="Float64x2Min"/>.</summary>
public class Float64x2RelaxedMin : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2RelaxedMin"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2RelaxedMin;

    /// <summary>Creates a new <see cref="Float64x2RelaxedMin"/> instance.</summary>
    public Float64x2RelaxedMin() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Float64x2Min.Execute(a, b);
}

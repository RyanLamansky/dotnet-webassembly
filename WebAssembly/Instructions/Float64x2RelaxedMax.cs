using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed f64x2 max; the deterministic profile is identical to <see cref="Float64x2Max"/>.</summary>
public class Float64x2RelaxedMax : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2RelaxedMax"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2RelaxedMax;

    /// <summary>Creates a new <see cref="Float64x2RelaxedMax"/> instance.</summary>
    public Float64x2RelaxedMax() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Float64x2Max.Execute(a, b);
}

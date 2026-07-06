using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed f32x4 max; the deterministic profile is identical to <see cref="Float32x4Max"/>.</summary>
public class Float32x4RelaxedMaximum : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4RelaxedMaximum"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4RelaxedMaximum;

    /// <summary>Creates a new <see cref="Float32x4RelaxedMaximum"/> instance.</summary>
    public Float32x4RelaxedMaximum() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Float32x4Max.Execute(a, b);
}

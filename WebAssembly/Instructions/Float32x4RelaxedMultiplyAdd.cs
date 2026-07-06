using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 fused multiply-add: each lane is <c>a*b + c</c> with a single rounding.</summary>
public class Float32x4RelaxedMultiplyAdd : SimdTernaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4RelaxedMultiplyAdd"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4RelaxedMultiplyAdd;

    /// <summary>Creates a new <see cref="Float32x4RelaxedMultiplyAdd"/> instance.</summary>
    public Float32x4RelaxedMultiplyAdd() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c)
    {
        var fa = a.AsSingle();
        var fb = b.AsSingle();
        var fc = c.AsSingle();
        Span<float> r = stackalloc float[4];
        for (var i = 0; i < 4; i++)
            r[i] = MathF.FusedMultiplyAdd(fa.GetElement(i), fb.GetElement(i), fc.GetElement(i));
        return Vector128.Create((ReadOnlySpan<float>)r).AsByte();
    }
}

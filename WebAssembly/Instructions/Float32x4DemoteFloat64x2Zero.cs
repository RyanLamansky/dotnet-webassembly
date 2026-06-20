using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4DemoteFloat64x2Zero instruction.</summary>
public class Float32x4DemoteFloat64x2Zero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4DemoteFloat64x2Zero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4DemoteFloat64x2Zero;

    /// <summary>Creates a new <see cref="Float32x4DemoteFloat64x2Zero"/> instance.</summary>
    public Float32x4DemoteFloat64x2Zero() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new float[4]; r[0] = (float)a.AsDouble().GetElement(0); r[1] = (float)a.AsDouble().GetElement(1); return Vector128.Create(r).AsByte(); }
}

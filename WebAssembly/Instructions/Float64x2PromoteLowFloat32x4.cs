using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2PromoteLowFloat32x4 instruction.</summary>
public class Float64x2PromoteLowFloat32x4 : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2PromoteLowFloat32x4"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2PromoteLowFloat32x4;

    /// <summary>Creates a new <see cref="Float64x2PromoteLowFloat32x4"/> instance.</summary>
    public Float64x2PromoteLowFloat32x4() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new double[2]; r[0] = a.AsSingle().GetElement(0); r[1] = a.AsSingle().GetElement(1); return Vector128.Create(r).AsByte(); }
}

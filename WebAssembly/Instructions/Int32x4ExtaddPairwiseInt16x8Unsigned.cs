using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i32x4 pairwise add unsigned.</summary>
public class Int32x4ExtaddPairwiseInt16x8Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtaddPairwiseInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtaddPairwiseInt16x8Unsigned;

    /// <summary>Creates a new <see cref="Int32x4ExtaddPairwiseInt16x8Unsigned"/> instance.</summary>
    public Int32x4ExtaddPairwiseInt16x8Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = (uint)(a.AsUInt16().GetElement(i*2) + a.AsUInt16().GetElement(i*2+1)); return Vector128.Create(r).AsByte(); }
}

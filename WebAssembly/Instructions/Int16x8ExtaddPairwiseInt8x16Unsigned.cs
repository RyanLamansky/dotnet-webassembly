using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ExtaddPairwiseInt8x16Unsigned instruction.</summary>
public class Int16x8ExtaddPairwiseInt8x16Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtaddPairwiseInt8x16Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtaddPairwiseInt8x16Unsigned;

    /// <summary>Creates a new <see cref="Int16x8ExtaddPairwiseInt8x16Unsigned"/> instance.</summary>
    public Int16x8ExtaddPairwiseInt8x16Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)(a.GetElement(i*2) + a.GetElement(i*2+1)); return Vector128.Create(r).AsByte(); }
}

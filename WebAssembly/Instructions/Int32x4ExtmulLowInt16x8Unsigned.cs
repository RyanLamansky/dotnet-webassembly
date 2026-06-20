using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtmulLowInt16x8Unsigned instruction.</summary>
public class Int32x4ExtmulLowInt16x8Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtmulLowInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtmulLowInt16x8Unsigned;

    /// <summary>Creates a new <see cref="Int32x4ExtmulLowInt16x8Unsigned"/> instance.</summary>
    public Int32x4ExtmulLowInt16x8Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new uint[4]; for (var i = 0; i < 4; i++) r[i] = (uint)(a.AsUInt16().GetElement(i) * b.AsUInt16().GetElement(i)); return Vector128.Create(r).AsByte(); }
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtmulLowInt32x4Unsigned instruction.</summary>
public class Int64x2ExtmulLowInt32x4Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtmulLowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtmulLowInt32x4Unsigned;

    /// <summary>Creates a new <see cref="Int64x2ExtmulLowInt32x4Unsigned"/> instance.</summary>
    public Int64x2ExtmulLowInt32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new ulong[2]; for (var i = 0; i < 2; i++) r[i] = (ulong)a.AsUInt32().GetElement(i) * b.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
}

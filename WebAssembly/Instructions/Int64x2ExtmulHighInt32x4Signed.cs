using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Multiply i64x2 lanes, signed extended from high i32x4.</summary>
public class Int64x2ExtmulHighInt32x4Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtmulHighInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtmulHighInt32x4Signed;

    /// <summary>Creates a new <see cref="Int64x2ExtmulHighInt32x4Signed"/> instance.</summary>
    public Int64x2ExtmulHighInt32x4Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new long[2]; for (var i = 0; i < 2; i++) r[i] = (long)a.AsInt32().GetElement(2+i) * b.AsInt32().GetElement(2+i); return Vector128.Create(r).AsByte(); }
}

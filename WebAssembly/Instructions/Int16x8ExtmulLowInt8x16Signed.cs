using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Multiply i16x8 lanes, accumulate to i32x4.</summary>
public class Int16x8ExtmulLowInt8x16Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtmulLowInt8x16Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtmulLowInt8x16Signed;

    /// <summary>Creates a new <see cref="Int16x8ExtmulLowInt8x16Signed"/> instance.</summary>
    public Int16x8ExtmulLowInt8x16Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new short[8]; for (var i = 0; i < 8; i++) r[i] = (short)((sbyte)a.GetElement(i) * (sbyte)b.GetElement(i)); return Vector128.Create(r).AsByte(); }
}

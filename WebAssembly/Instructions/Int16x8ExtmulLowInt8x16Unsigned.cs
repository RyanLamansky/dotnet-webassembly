using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Multiply low unsigned i16x8 lanes.</summary>
public class Int16x8ExtmulLowInt8x16Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned;

    /// <summary>Creates a new <see cref="Int16x8ExtmulLowInt8x16Unsigned"/> instance.</summary>
    public Int16x8ExtmulLowInt8x16Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)(a.GetElement(i) * b.GetElement(i)); return Vector128.Create(r).AsByte(); }
}

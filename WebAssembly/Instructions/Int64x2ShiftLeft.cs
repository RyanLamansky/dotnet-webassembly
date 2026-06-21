using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i64x2 shift left.</summary>
public class Int64x2ShiftLeft : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftLeft;

    /// <summary>Creates a new <see cref="Int64x2ShiftLeft"/> instance.</summary>
    public Int64x2ShiftLeft() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftLeft(a.AsInt64(), shift & 63).AsByte();
}

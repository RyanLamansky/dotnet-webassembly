using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftRightUnsigned instruction.</summary>
public class Int64x2ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftRightUnsigned;

    /// <summary>Creates a new <see cref="Int64x2ShiftRightUnsigned"/> instance.</summary>
    public Int64x2ShiftRightUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftRightLogical(a.AsUInt64(), shift & 63).AsByte();
}

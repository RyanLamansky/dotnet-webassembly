using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftRightSigned instruction.</summary>
public class Int64x2ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftRightSigned;

    /// <summary>Creates a new <see cref="Int64x2ShiftRightSigned"/> instance.</summary>
    public Int64x2ShiftRightSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftRightArithmetic(a.AsInt64(), shift & 63).AsByte();
}

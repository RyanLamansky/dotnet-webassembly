using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 unsigned shift right.</summary>
public class Int16x8ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftRightUnsigned;

    /// <summary>Creates a new <see cref="Int16x8ShiftRightUnsigned"/> instance.</summary>
    public Int16x8ShiftRightUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftRightLogical(a.AsUInt16(), shift & 15).AsByte();
}

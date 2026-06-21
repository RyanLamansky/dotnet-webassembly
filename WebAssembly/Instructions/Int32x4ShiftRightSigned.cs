using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i32x4 signed shift right.</summary>
public class Int32x4ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftRightSigned;

    /// <summary>Creates a new <see cref="Int32x4ShiftRightSigned"/> instance.</summary>
    public Int32x4ShiftRightSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftRightArithmetic(a.AsInt32(), shift & 31).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ShiftRightUnsigned instruction.</summary>
public class Int32x4ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftRightUnsigned;

    /// <summary>Creates a new <see cref="Int32x4ShiftRightUnsigned"/> instance.</summary>
    public Int32x4ShiftRightUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftRightLogical(a.AsUInt32(), shift & 31).AsByte();
}

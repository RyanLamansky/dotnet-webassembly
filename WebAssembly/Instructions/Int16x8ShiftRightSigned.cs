using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftRightSigned instruction.</summary>
public class Int16x8ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftRightSigned;

    /// <summary>Creates a new <see cref="Int16x8ShiftRightSigned"/> instance.</summary>
    public Int16x8ShiftRightSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftRightArithmetic(a.AsInt16(), shift & 15).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftRightUnsigned instruction.</summary>
public class Int8x16ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftRightUnsigned;

    /// <summary>Creates a new <see cref="Int8x16ShiftRightUnsigned"/> instance.</summary>
    public Int8x16ShiftRightUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) { shift &= 7; var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)(a.GetElement(i) >> shift); return Vector128.Create(r); }
}

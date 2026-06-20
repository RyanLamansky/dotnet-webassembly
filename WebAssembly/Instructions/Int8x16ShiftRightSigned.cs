using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftRightSigned instruction.</summary>
public class Int8x16ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftRightSigned;

    /// <summary>Creates a new <see cref="Int8x16ShiftRightSigned"/> instance.</summary>
    public Int8x16ShiftRightSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) { shift &= 7; var r = new sbyte[16]; for (var i = 0; i < 16; i++) r[i] = (sbyte)(a.AsSByte().GetElement(i) >> shift); return Vector128.Create(r).AsByte(); }
}

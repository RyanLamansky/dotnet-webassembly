using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ShiftLeft instruction.</summary>
public class Int32x4ShiftLeft : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftLeft;

    /// <summary>Creates a new <see cref="Int32x4ShiftLeft"/> instance.</summary>
    public Int32x4ShiftLeft() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftLeft(a.AsInt32(), shift & 31).AsByte();
}

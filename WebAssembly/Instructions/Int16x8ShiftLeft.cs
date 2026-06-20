using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftLeft instruction.</summary>
public class Int16x8ShiftLeft : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftLeft;

    /// <summary>Creates a new <see cref="Int16x8ShiftLeft"/> instance.</summary>
    public Int16x8ShiftLeft() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, int shift) => Vector128.ShiftLeft(a.AsInt16(), shift & 15).AsByte();
}

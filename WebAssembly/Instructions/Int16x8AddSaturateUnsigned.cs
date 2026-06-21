using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 unsigned add with saturation.</summary>
public class Int16x8AddSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AddSaturateUnsigned;

    /// <summary>Creates a new <see cref="Int16x8AddSaturateUnsigned"/> instance.</summary>
    public Int16x8AddSaturateUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new ushort[8];
        for (var i = 0; i < 8; i++) { var v = a.AsUInt16().GetElement(i) + b.AsUInt16().GetElement(i); r[i] = v > 65535u ? (ushort)65535 : (ushort)v; }
        return Vector128.Create(r).AsByte();
    }
}

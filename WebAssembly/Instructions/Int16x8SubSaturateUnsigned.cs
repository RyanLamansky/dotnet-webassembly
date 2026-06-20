using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8SubSaturateUnsigned instruction.</summary>
public class Int16x8SubSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8SubSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8SubSaturateUnsigned;

    /// <summary>Creates a new <see cref="Int16x8SubSaturateUnsigned"/> instance.</summary>
    public Int16x8SubSaturateUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new ushort[8];
        for (var i = 0; i < 8; i++) { var x = a.AsUInt16().GetElement(i); var y = b.AsUInt16().GetElement(i); r[i] = x < y ? (ushort)0 : (ushort)(x - y); }
        return Vector128.Create(r).AsByte();
    }
}

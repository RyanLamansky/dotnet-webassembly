using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16SubSaturateUnsigned instruction.</summary>
public class Int8x16SubSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16SubSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16SubSaturateUnsigned;

    /// <summary>Creates a new <see cref="Int8x16SubSaturateUnsigned"/> instance.</summary>
    public Int8x16SubSaturateUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new byte[16];
        for (var i = 0; i < 16; i++) { var x = a.GetElement(i); var y = b.GetElement(i); r[i] = x < y ? (byte)0 : (byte)(x - y); }
        return Vector128.Create(r);
    }
}

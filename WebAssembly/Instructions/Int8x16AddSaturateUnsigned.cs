using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AddSaturateUnsigned instruction.</summary>
public class Int8x16AddSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AddSaturateUnsigned;

    /// <summary>Creates a new <see cref="Int8x16AddSaturateUnsigned"/> instance.</summary>
    public Int8x16AddSaturateUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new byte[16];
        for (var i = 0; i < 16; i++) { var v = a.GetElement(i) + b.GetElement(i); r[i] = v > 255 ? (byte)255 : (byte)v; }
        return Vector128.Create(r);
    }
}

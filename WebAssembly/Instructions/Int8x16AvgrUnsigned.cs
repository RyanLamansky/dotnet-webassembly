using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AvgrUnsigned instruction.</summary>
public class Int8x16AvgrUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AvgrUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AvgrUnsigned;

    /// <summary>Creates a new <see cref="Int8x16AvgrUnsigned"/> instance.</summary>
    public Int8x16AvgrUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)((a.GetElement(i) + b.GetElement(i) + 1) >> 1); return Vector128.Create(r); }
}

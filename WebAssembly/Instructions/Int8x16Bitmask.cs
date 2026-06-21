using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitmask of MSB of each i8x16 lane.</summary>
public class Int8x16Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Bitmask;

    /// <summary>Creates a new <see cref="Int8x16Bitmask"/> instance.</summary>
    public Int8x16Bitmask() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) { var r = 0; for (var i = 0; i < 16; i++) if ((a.GetElement(i) >> 7) != 0) r |= 1 << i; return r; }
}

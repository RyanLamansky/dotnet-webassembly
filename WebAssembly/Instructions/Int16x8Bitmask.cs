using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitmask of MSB of each i16x8 lane.</summary>
public class Int16x8Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Bitmask;

    /// <summary>Creates a new <see cref="Int16x8Bitmask"/> instance.</summary>
    public Int16x8Bitmask() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) { var r = 0; for (var i = 0; i < 8; i++) if ((a.AsUInt16().GetElement(i) >> 15) != 0) r |= 1 << i; return r; }
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitmask of MSB of each i32x4 lane.</summary>
public class Int32x4Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Bitmask;

    /// <summary>Creates a new <see cref="Int32x4Bitmask"/> instance.</summary>
    public Int32x4Bitmask() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) { var r = 0; for (var i = 0; i < 4; i++) if (((uint)a.AsInt32().GetElement(i) >> 31) != 0) r |= 1 << i; return r; }
}

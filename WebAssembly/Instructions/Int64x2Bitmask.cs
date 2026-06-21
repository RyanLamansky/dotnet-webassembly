using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitmask of MSB of each i64x2 lane.</summary>
public class Int64x2Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Bitmask;

    /// <summary>Creates a new <see cref="Int64x2Bitmask"/> instance.</summary>
    public Int64x2Bitmask() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) { var r = 0; for (var i = 0; i < 2; i++) if (((ulong)a.AsInt64().GetElement(i) >> 63) != 0) r |= 1 << i; return r; }
}

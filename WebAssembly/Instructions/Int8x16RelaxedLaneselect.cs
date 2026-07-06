using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed i8x16 lane select; the deterministic profile is identical to <see cref="V128Bitselect"/>.</summary>
public class Int8x16RelaxedLaneselect : SimdTernaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16RelaxedLaneselect"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16RelaxedLaneselect;

    /// <summary>Creates a new <see cref="Int8x16RelaxedLaneselect"/> instance.</summary>
    public Int8x16RelaxedLaneselect() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b, Vector128<byte> mask) => V128Bitselect.Execute(a, b, mask);
}

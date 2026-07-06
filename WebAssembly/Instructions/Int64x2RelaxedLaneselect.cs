using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed i64x2 lane select; the deterministic profile is identical to <see cref="V128Bitselect"/>.</summary>
public class Int64x2RelaxedLaneselect : SimdTernaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2RelaxedLaneselect"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2RelaxedLaneselect;

    /// <summary>Creates a new <see cref="Int64x2RelaxedLaneselect"/> instance.</summary>
    public Int64x2RelaxedLaneselect() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b, Vector128<byte> mask) => V128Bitselect.Execute(a, b, mask);
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed i32x4 lane select; the deterministic profile is identical to <see cref="V128Bitselect"/>.</summary>
public class Int32x4RelaxedLaneselect : SimdTernaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4RelaxedLaneselect"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4RelaxedLaneselect;

    /// <summary>Creates a new <see cref="Int32x4RelaxedLaneselect"/> instance.</summary>
    public Int32x4RelaxedLaneselect() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b, Vector128<byte> mask) => V128Bitselect.Execute(a, b, mask);
}

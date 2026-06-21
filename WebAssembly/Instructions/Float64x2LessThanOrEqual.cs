using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 less-than-or-equal.</summary>
public class Float64x2LessThanOrEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2LessThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2LessThanOrEqual;

    /// <summary>Creates a new <see cref="Float64x2LessThanOrEqual"/> instance.</summary>
    public Float64x2LessThanOrEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsDouble(), b.AsDouble()).AsByte();
}

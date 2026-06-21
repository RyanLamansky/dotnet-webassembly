using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 greater-than.</summary>
public class Float64x2GreaterThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2GreaterThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2GreaterThan;

    /// <summary>Creates a new <see cref="Float64x2GreaterThan"/> instance.</summary>
    public Float64x2GreaterThan() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsDouble(), b.AsDouble()).AsByte();
}

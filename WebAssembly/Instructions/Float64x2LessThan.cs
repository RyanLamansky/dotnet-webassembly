using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 less-than.</summary>
public class Float64x2LessThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2LessThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2LessThan;

    /// <summary>Creates a new <see cref="Float64x2LessThan"/> instance.</summary>
    public Float64x2LessThan() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsDouble(), b.AsDouble()).AsByte();
}

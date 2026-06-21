using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 divide.</summary>
public class Float64x2Div : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Div"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Div;

    /// <summary>Creates a new <see cref="Float64x2Div"/> instance.</summary>
    public Float64x2Div() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() / b.AsDouble()).AsByte();
}

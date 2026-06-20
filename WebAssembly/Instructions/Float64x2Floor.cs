using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Floor instruction.</summary>
public class Float64x2Floor : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Floor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Floor;

    /// <summary>Creates a new <see cref="Float64x2Floor"/> instance.</summary>
    public Float64x2Floor() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Floor(a.AsDouble()).AsByte();
}

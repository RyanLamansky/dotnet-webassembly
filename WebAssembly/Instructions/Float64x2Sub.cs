using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Sub instruction.</summary>
public class Float64x2Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Sub;

    /// <summary>Creates a new <see cref="Float64x2Sub"/> instance.</summary>
    public Float64x2Sub() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() - b.AsDouble()).AsByte();
}

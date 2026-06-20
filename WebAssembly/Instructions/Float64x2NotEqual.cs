using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2NotEqual instruction.</summary>
public class Float64x2NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2NotEqual;

    /// <summary>Creates a new <see cref="Float64x2NotEqual"/> instance.</summary>
    public Float64x2NotEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsDouble(), b.AsDouble())).AsByte();
}

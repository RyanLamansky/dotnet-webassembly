using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4GreaterThanOrEqual instruction.</summary>
public class Float32x4GreaterThanOrEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4GreaterThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4GreaterThanOrEqual;

    /// <summary>Creates a new <see cref="Float32x4GreaterThanOrEqual"/> instance.</summary>
    public Float32x4GreaterThanOrEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsSingle(), b.AsSingle()).AsByte();
}

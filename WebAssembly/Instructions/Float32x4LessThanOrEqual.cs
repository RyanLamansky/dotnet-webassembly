using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 less-than-or-equal.</summary>
public class Float32x4LessThanOrEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4LessThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4LessThanOrEqual;

    /// <summary>Creates a new <see cref="Float32x4LessThanOrEqual"/> instance.</summary>
    public Float32x4LessThanOrEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsSingle(), b.AsSingle()).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 not equal.</summary>
public class Float32x4NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4NotEqual;

    /// <summary>Creates a new <see cref="Float32x4NotEqual"/> instance.</summary>
    public Float32x4NotEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsSingle(), b.AsSingle())).AsByte();
}

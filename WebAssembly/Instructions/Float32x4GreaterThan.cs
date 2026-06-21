using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 greater-than.</summary>
public class Float32x4GreaterThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4GreaterThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4GreaterThan;

    /// <summary>Creates a new <see cref="Float32x4GreaterThan"/> instance.</summary>
    public Float32x4GreaterThan() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsSingle(), b.AsSingle()).AsByte();
}

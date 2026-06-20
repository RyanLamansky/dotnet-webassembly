using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4LessThan instruction.</summary>
public class Float32x4LessThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4LessThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4LessThan;

    /// <summary>Creates a new <see cref="Float32x4LessThan"/> instance.</summary>
    public Float32x4LessThan() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThan(a.AsSingle(), b.AsSingle()).AsByte();
}

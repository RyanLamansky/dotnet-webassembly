using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Div instruction.</summary>
public class Float32x4Div : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Div"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Div;

    /// <summary>Creates a new <see cref="Float32x4Div"/> instance.</summary>
    public Float32x4Div() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() / b.AsSingle()).AsByte();
}

using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 add.</summary>
public class Float32x4Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Add;

    /// <summary>Creates a new <see cref="Float32x4Add"/> instance.</summary>
    public Float32x4Add() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() + b.AsSingle()).AsByte();
}

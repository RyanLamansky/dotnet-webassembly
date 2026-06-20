using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Add instruction.</summary>
public class Float64x2Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Add;

    /// <summary>Creates a new <see cref="Float64x2Add"/> instance.</summary>
    public Float64x2Add() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsDouble() + b.AsDouble()).AsByte();
}

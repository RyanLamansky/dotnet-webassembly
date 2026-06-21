using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i8x16 add.</summary>
public class Int8x16Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Add;

    /// <summary>Creates a new <see cref="Int8x16Add"/> instance.</summary>
    public Int8x16Add() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsSByte() + b.AsSByte()).AsByte();
}

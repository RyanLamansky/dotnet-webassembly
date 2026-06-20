using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Add instruction.</summary>
public class Int16x8Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Add;

    /// <summary>Creates a new <see cref="Int16x8Add"/> instance.</summary>
    public Int16x8Add() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsInt16() + b.AsInt16()).AsByte();
}

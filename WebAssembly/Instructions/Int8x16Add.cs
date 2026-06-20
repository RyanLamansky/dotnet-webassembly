using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Add instruction.</summary>
public class Int8x16Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AddMethod;

    /// <summary>Creates a new <see cref="Int8x16Add"/> instance.</summary>
    public Int8x16Add() { }
}

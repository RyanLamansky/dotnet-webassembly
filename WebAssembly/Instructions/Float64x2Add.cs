using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Add instruction.</summary>
public class Float64x2Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2AddMethod;

    /// <summary>Creates a new <see cref="Float64x2Add"/> instance.</summary>
    public Float64x2Add() { }
}

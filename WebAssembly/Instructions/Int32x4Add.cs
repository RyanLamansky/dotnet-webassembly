using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Add instruction.</summary>
public class Int32x4Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4AddMethod;

    /// <summary>Creates a new <see cref="Int32x4Add"/> instance.</summary>
    public Int32x4Add() { }
}

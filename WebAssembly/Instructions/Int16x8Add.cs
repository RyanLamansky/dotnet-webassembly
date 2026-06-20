using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Add instruction.</summary>
public class Int16x8Add : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AddMethod;

    /// <summary>Creates a new <see cref="Int16x8Add"/> instance.</summary>
    public Int16x8Add() { }
}

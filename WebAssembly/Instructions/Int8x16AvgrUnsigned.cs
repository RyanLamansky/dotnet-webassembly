using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AvgrUnsigned instruction.</summary>
public class Int8x16AvgrUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AvgrUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AvgrUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AvgrUMethod;

    /// <summary>Creates a new <see cref="Int8x16AvgrUnsigned"/> instance.</summary>
    public Int8x16AvgrUnsigned() { }
}

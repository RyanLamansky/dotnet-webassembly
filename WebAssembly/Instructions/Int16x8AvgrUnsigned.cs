using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AvgrUnsigned instruction.</summary>
public class Int16x8AvgrUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AvgrUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AvgrUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AvgrUMethod;

    /// <summary>Creates a new <see cref="Int16x8AvgrUnsigned"/> instance.</summary>
    public Int16x8AvgrUnsigned() { }
}

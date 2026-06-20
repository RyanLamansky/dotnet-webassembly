using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftLeft instruction.</summary>
public class Int8x16ShiftLeft : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftLeft;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ShlMethod;

    /// <summary>Creates a new <see cref="Int8x16ShiftLeft"/> instance.</summary>
    public Int8x16ShiftLeft() { }
}

using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftLeft instruction.</summary>
public class Int64x2ShiftLeft : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftLeft;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ShlMethod;

    /// <summary>Creates a new <see cref="Int64x2ShiftLeft"/> instance.</summary>
    public Int64x2ShiftLeft() { }
}

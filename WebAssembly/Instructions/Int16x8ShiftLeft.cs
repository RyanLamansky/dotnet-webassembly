using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftLeft instruction.</summary>
public class Int16x8ShiftLeft : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftLeft;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ShlMethod;

    /// <summary>Creates a new <see cref="Int16x8ShiftLeft"/> instance.</summary>
    public Int16x8ShiftLeft() { }
}

using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftRightUnsigned instruction.</summary>
public class Int16x8ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftRightUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ShrUMethod;

    /// <summary>Creates a new <see cref="Int16x8ShiftRightUnsigned"/> instance.</summary>
    public Int16x8ShiftRightUnsigned() { }
}

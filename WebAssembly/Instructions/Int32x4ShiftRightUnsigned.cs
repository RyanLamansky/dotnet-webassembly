using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ShiftRightUnsigned instruction.</summary>
public class Int32x4ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftRightUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ShrUMethod;

    /// <summary>Creates a new <see cref="Int32x4ShiftRightUnsigned"/> instance.</summary>
    public Int32x4ShiftRightUnsigned() { }
}

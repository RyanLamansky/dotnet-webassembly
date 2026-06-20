using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ShiftRightSigned instruction.</summary>
public class Int32x4ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ShrSMethod;

    /// <summary>Creates a new <see cref="Int32x4ShiftRightSigned"/> instance.</summary>
    public Int32x4ShiftRightSigned() { }
}

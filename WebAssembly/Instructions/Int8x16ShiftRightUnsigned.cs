using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftRightUnsigned instruction.</summary>
public class Int8x16ShiftRightUnsigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftRightUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ShrUMethod;

    /// <summary>Creates a new <see cref="Int8x16ShiftRightUnsigned"/> instance.</summary>
    public Int8x16ShiftRightUnsigned() { }
}

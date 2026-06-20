using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftRightSigned instruction.</summary>
public class Int8x16ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ShrSMethod;

    /// <summary>Creates a new <see cref="Int8x16ShiftRightSigned"/> instance.</summary>
    public Int8x16ShiftRightSigned() { }
}

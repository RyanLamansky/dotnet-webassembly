using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftRightSigned instruction.</summary>
public class Int64x2ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ShrSMethod;

    /// <summary>Creates a new <see cref="Int64x2ShiftRightSigned"/> instance.</summary>
    public Int64x2ShiftRightSigned() { }
}

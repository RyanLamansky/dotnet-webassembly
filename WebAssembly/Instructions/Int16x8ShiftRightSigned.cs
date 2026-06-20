using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftRightSigned instruction.</summary>
public class Int16x8ShiftRightSigned : SimdShiftInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ShrSMethod;

    /// <summary>Creates a new <see cref="Int16x8ShiftRightSigned"/> instance.</summary>
    public Int16x8ShiftRightSigned() { }
}

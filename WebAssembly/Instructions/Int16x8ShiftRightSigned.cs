using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftRightSigned instruction.</summary>
public class Int16x8ShiftRightSigned : SimdShiftInstruction, IEquatable<Int16x8ShiftRightSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ShrSMethod;

    /// <summary>Creates a new <see cref="Int16x8ShiftRightSigned"/> instance.</summary>
    public Int16x8ShiftRightSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8ShiftRightSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8ShiftRightSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8ShiftRightSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8ShiftRightSigned;
}

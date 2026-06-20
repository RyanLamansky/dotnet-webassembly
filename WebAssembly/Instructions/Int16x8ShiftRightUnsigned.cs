using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ShiftRightUnsigned instruction.</summary>
public class Int16x8ShiftRightUnsigned : SimdShiftInstruction, IEquatable<Int16x8ShiftRightUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ShiftRightUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ShrUMethod;

    /// <summary>Creates a new <see cref="Int16x8ShiftRightUnsigned"/> instance.</summary>
    public Int16x8ShiftRightUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8ShiftRightUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8ShiftRightUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8ShiftRightUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8ShiftRightUnsigned;
}

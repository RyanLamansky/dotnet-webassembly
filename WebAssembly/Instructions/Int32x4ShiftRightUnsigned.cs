using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ShiftRightUnsigned instruction.</summary>
public class Int32x4ShiftRightUnsigned : SimdShiftInstruction, IEquatable<Int32x4ShiftRightUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftRightUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ShrUMethod;

    /// <summary>Creates a new <see cref="Int32x4ShiftRightUnsigned"/> instance.</summary>
    public Int32x4ShiftRightUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4ShiftRightUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4ShiftRightUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4ShiftRightUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4ShiftRightUnsigned;
}

using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftRightUnsigned instruction.</summary>
public class Int64x2ShiftRightUnsigned : SimdShiftInstruction, IEquatable<Int64x2ShiftRightUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftRightUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftRightUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ShrUMethod;

    /// <summary>Creates a new <see cref="Int64x2ShiftRightUnsigned"/> instance.</summary>
    public Int64x2ShiftRightUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2ShiftRightUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int64x2ShiftRightUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2ShiftRightUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2ShiftRightUnsigned;
}

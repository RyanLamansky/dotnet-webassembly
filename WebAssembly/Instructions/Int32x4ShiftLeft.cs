using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ShiftLeft instruction.</summary>
public class Int32x4ShiftLeft : SimdShiftInstruction, IEquatable<Int32x4ShiftLeft>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ShiftLeft;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ShlMethod;

    /// <summary>Creates a new <see cref="Int32x4ShiftLeft"/> instance.</summary>
    public Int32x4ShiftLeft() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4ShiftLeft;
    /// <inheritdoc/>
    public bool Equals(Int32x4ShiftLeft? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4ShiftLeft;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4ShiftLeft;
}

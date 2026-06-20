using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftLeft instruction.</summary>
public class Int64x2ShiftLeft : SimdShiftInstruction, IEquatable<Int64x2ShiftLeft>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftLeft;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ShlMethod;

    /// <summary>Creates a new <see cref="Int64x2ShiftLeft"/> instance.</summary>
    public Int64x2ShiftLeft() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2ShiftLeft;
    /// <inheritdoc/>
    public bool Equals(Int64x2ShiftLeft? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2ShiftLeft;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2ShiftLeft;
}

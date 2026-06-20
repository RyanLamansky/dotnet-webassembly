using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftLeft instruction.</summary>
public class Int8x16ShiftLeft : SimdShiftInstruction, IEquatable<Int8x16ShiftLeft>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftLeft"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftLeft;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ShlMethod;

    /// <summary>Creates a new <see cref="Int8x16ShiftLeft"/> instance.</summary>
    public Int8x16ShiftLeft() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16ShiftLeft;
    /// <inheritdoc/>
    public bool Equals(Int8x16ShiftLeft? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16ShiftLeft;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16ShiftLeft;
}

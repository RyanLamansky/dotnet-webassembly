using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ShiftRightSigned instruction.</summary>
public class Int64x2ShiftRightSigned : SimdShiftInstruction, IEquatable<Int64x2ShiftRightSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ShrSMethod;

    /// <summary>Creates a new <see cref="Int64x2ShiftRightSigned"/> instance.</summary>
    public Int64x2ShiftRightSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2ShiftRightSigned;
    /// <inheritdoc/>
    public bool Equals(Int64x2ShiftRightSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2ShiftRightSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2ShiftRightSigned;
}

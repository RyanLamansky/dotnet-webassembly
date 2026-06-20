using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ShiftRightSigned instruction.</summary>
public class Int8x16ShiftRightSigned : SimdShiftInstruction, IEquatable<Int8x16ShiftRightSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ShiftRightSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ShiftRightSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ShrSMethod;

    /// <summary>Creates a new <see cref="Int8x16ShiftRightSigned"/> instance.</summary>
    public Int8x16ShiftRightSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16ShiftRightSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16ShiftRightSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16ShiftRightSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16ShiftRightSigned;
}

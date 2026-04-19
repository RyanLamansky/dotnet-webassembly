using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Sub instruction.</summary>
public class Int16x8Sub : SimdBinaryV128Instruction, IEquatable<Int16x8Sub>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SubMethod;

    /// <summary>Creates a new <see cref="Int16x8Sub"/> instance.</summary>
    public Int16x8Sub() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8Sub;
    /// <inheritdoc/>
    public bool Equals(Int16x8Sub? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8Sub;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8Sub;
}

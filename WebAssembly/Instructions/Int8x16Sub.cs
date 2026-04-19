using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Sub instruction.</summary>
public class Int8x16Sub : SimdBinaryV128Instruction, IEquatable<Int8x16Sub>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SubMethod;

    /// <summary>Creates a new <see cref="Int8x16Sub"/> instance.</summary>
    public Int8x16Sub() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Sub;
    /// <inheritdoc/>
    public bool Equals(Int8x16Sub? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Sub;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Sub;
}

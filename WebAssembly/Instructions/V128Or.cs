using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Bitwise OR of two v128 values.</summary>
public class V128Or : SimdBinaryV128Instruction, IEquatable<V128Or>
{
    /// <summary>Always <see cref="SimdOpCode.V128Or"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Or;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.V128OrMethod;

    /// <summary>Creates a new <see cref="V128Or"/> instance.</summary>
    public V128Or() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is V128Or;
    /// <inheritdoc/>
    public bool Equals(V128Or? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is V128Or;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.V128Or;
}

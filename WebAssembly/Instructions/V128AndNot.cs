using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Bitwise ANDNOT (a &amp; ~b) of two v128 values.</summary>
public class V128AndNot : SimdBinaryV128Instruction, IEquatable<V128AndNot>
{
    /// <summary>Always <see cref="SimdOpCode.V128AndNot"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128AndNot;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.V128AndNotMethod;

    /// <summary>Creates a new <see cref="V128AndNot"/> instance.</summary>
    public V128AndNot() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is V128AndNot;
    /// <inheritdoc/>
    public bool Equals(V128AndNot? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is V128AndNot;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.V128AndNot;
}

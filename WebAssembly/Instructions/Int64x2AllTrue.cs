using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2AllTrue instruction.</summary>
public class Int64x2AllTrue : SimdV128ToI32Instruction, IEquatable<Int64x2AllTrue>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2AllTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2AllTrueMethod;

    /// <summary>Creates a new <see cref="Int64x2AllTrue"/> instance.</summary>
    public Int64x2AllTrue() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2AllTrue;
    /// <inheritdoc/>
    public bool Equals(Int64x2AllTrue? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2AllTrue;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2AllTrue;
}

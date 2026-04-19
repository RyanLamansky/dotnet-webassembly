using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AllTrue instruction.</summary>
public class Int8x16AllTrue : SimdV128ToI32Instruction, IEquatable<Int8x16AllTrue>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AllTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AllTrueMethod;

    /// <summary>Creates a new <see cref="Int8x16AllTrue"/> instance.</summary>
    public Int8x16AllTrue() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16AllTrue;
    /// <inheritdoc/>
    public bool Equals(Int8x16AllTrue? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16AllTrue;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16AllTrue;
}

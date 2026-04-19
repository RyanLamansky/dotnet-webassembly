using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AllTrue instruction.</summary>
public class Int16x8AllTrue : SimdV128ToI32Instruction, IEquatable<Int16x8AllTrue>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AllTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AllTrueMethod;

    /// <summary>Creates a new <see cref="Int16x8AllTrue"/> instance.</summary>
    public Int16x8AllTrue() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8AllTrue;
    /// <inheritdoc/>
    public bool Equals(Int16x8AllTrue? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8AllTrue;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8AllTrue;
}

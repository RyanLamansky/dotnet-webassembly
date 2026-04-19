using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AvgrUnsigned instruction.</summary>
public class Int16x8AvgrUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8AvgrUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AvgrUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AvgrUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AvgrUMethod;

    /// <summary>Creates a new <see cref="Int16x8AvgrUnsigned"/> instance.</summary>
    public Int16x8AvgrUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8AvgrUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8AvgrUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8AvgrUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8AvgrUnsigned;
}

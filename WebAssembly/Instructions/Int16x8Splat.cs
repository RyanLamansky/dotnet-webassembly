using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Splat instruction.</summary>
public class Int16x8Splat : SimdSplatInstruction, IEquatable<Int16x8Splat>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SplatMethod;

    /// <summary>Creates a new <see cref="Int16x8Splat"/> instance.</summary>
    public Int16x8Splat() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8Splat;
    /// <inheritdoc/>
    public bool Equals(Int16x8Splat? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8Splat;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8Splat;
}

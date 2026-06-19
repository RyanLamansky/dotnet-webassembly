using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Splat instruction.</summary>
public class Int32x4Splat : SimdSplatInstruction, IEquatable<Int32x4Splat>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4SplatMethod;

    /// <summary>Creates a new <see cref="Int32x4Splat"/> instance.</summary>
    public Int32x4Splat() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4Splat;
    /// <inheritdoc/>
    public bool Equals(Int32x4Splat? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4Splat;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4Splat;
}

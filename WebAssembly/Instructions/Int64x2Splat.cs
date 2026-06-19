using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Splat instruction.</summary>
public class Int64x2Splat : SimdSplatInstruction, IEquatable<Int64x2Splat>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2SplatMethod;

    /// <summary>Creates a new <see cref="Int64x2Splat"/> instance.</summary>
    public Int64x2Splat() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2Splat;
    /// <inheritdoc/>
    public bool Equals(Int64x2Splat? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2Splat;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2Splat;
}

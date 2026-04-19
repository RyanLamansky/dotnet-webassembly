using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ReplaceLane instruction.</summary>
public class Int64x2ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Int64x2ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ReplaceLaneMethod;

    /// <summary>Creates a new <see cref="Int64x2ReplaceLane"/> instance.</summary>
    public Int64x2ReplaceLane() { }
    internal Int64x2ReplaceLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int64x2ReplaceLane);
    /// <inheritdoc/>
    public bool Equals(Int64x2ReplaceLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int64x2ReplaceLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

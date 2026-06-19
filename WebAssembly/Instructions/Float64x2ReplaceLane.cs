using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2ReplaceLane instruction.</summary>
public class Float64x2ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Float64x2ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2ReplaceLaneMethod;
    internal override byte MaxLaneCount => 2;

    /// <summary>Creates a new <see cref="Float64x2ReplaceLane"/> instance.</summary>
    public Float64x2ReplaceLane() { }
    internal Float64x2ReplaceLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Float64x2ReplaceLane);
    /// <inheritdoc/>
    public bool Equals(Float64x2ReplaceLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Float64x2ReplaceLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

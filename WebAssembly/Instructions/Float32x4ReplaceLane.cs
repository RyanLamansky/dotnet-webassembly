using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4ReplaceLane instruction.</summary>
public class Float32x4ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Float32x4ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4ReplaceLaneMethod;
    internal override byte MaxLaneCount => 4;

    /// <summary>Creates a new <see cref="Float32x4ReplaceLane"/> instance.</summary>
    public Float32x4ReplaceLane() { }
    internal Float32x4ReplaceLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Float32x4ReplaceLane);
    /// <inheritdoc/>
    public bool Equals(Float32x4ReplaceLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Float32x4ReplaceLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

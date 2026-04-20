using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ReplaceLane instruction.</summary>
public class Int16x8ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Int16x8ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ReplaceLaneMethod;
    internal override byte MaxLaneCount => 8;

    /// <summary>Creates a new <see cref="Int16x8ReplaceLane"/> instance.</summary>
    public Int16x8ReplaceLane() { }
    internal Int16x8ReplaceLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int16x8ReplaceLane);
    /// <inheritdoc/>
    public bool Equals(Int16x8ReplaceLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int16x8ReplaceLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

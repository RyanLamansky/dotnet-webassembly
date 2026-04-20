using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4ExtractLane instruction.</summary>
public class Float32x4ExtractLane : SimdExtractLaneInstruction, IEquatable<Float32x4ExtractLane>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4ExtractLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4ExtractLane;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Float32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4ExtractLaneMethod;
    internal override byte MaxLaneCount => 4;

    /// <summary>Creates a new <see cref="Float32x4ExtractLane"/> instance.</summary>
    public Float32x4ExtractLane() { }
    internal Float32x4ExtractLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Float32x4ExtractLane);
    /// <inheritdoc/>
    public bool Equals(Float32x4ExtractLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Float32x4ExtractLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

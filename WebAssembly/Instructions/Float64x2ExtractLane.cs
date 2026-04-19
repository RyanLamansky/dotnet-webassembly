using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2ExtractLane instruction.</summary>
public class Float64x2ExtractLane : SimdExtractLaneInstruction, IEquatable<Float64x2ExtractLane>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2ExtractLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2ExtractLane;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Float64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2ExtractLaneMethod;

    /// <summary>Creates a new <see cref="Float64x2ExtractLane"/> instance.</summary>
    public Float64x2ExtractLane() { }
    internal Float64x2ExtractLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Float64x2ExtractLane);
    /// <inheritdoc/>
    public bool Equals(Float64x2ExtractLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Float64x2ExtractLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

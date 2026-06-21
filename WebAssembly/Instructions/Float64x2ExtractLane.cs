using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Extract an f64 lane.</summary>
public class Float64x2ExtractLane : SimdExtractLaneInstruction, IEquatable<Float64x2ExtractLane>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2ExtractLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2ExtractLane;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Float64;
    internal override byte MaxLaneCount => 2;

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

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static double Execute(Vector128<byte> v, int lane) => v.AsDouble().GetElement(lane);
}

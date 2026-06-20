using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtractLane instruction.</summary>
public class Int64x2ExtractLane : SimdExtractLaneInstruction, IEquatable<Int64x2ExtractLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtractLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtractLane;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int64;
    internal override byte MaxLaneCount => 2;

    /// <summary>Creates a new <see cref="Int64x2ExtractLane"/> instance.</summary>
    public Int64x2ExtractLane() { }
    internal Int64x2ExtractLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int64x2ExtractLane);
    /// <inheritdoc/>
    public bool Equals(Int64x2ExtractLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int64x2ExtractLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static long Execute(Vector128<byte> v, int lane) => v.AsInt64().GetElement(lane);
}

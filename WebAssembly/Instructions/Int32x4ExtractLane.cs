using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Extract an i32 lane.</summary>
public class Int32x4ExtractLane : SimdExtractLaneInstruction, IEquatable<Int32x4ExtractLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtractLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtractLane;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override byte MaxLaneCount => 4;

    /// <summary>Creates a new <see cref="Int32x4ExtractLane"/> instance.</summary>
    public Int32x4ExtractLane() { }
    internal Int32x4ExtractLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int32x4ExtractLane);
    /// <inheritdoc/>
    public bool Equals(Int32x4ExtractLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int32x4ExtractLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> v, int lane) => v.AsInt32().GetElement(lane);
}

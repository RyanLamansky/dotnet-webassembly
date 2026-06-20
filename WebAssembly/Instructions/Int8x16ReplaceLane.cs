using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ReplaceLane instruction.</summary>
public class Int8x16ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Int8x16ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override byte MaxLaneCount => 16;

    /// <summary>Creates a new <see cref="Int8x16ReplaceLane"/> instance.</summary>
    public Int8x16ReplaceLane() { }
    internal Int8x16ReplaceLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int8x16ReplaceLane);
    /// <inheritdoc/>
    public bool Equals(Int8x16ReplaceLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int8x16ReplaceLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> v, int lane, int x) => v.WithElement(lane, (byte)(x & 0xFF));
}

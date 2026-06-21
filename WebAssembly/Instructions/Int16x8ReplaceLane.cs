using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Replace an i16x8 lane with an i32 value.</summary>
public class Int16x8ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Int16x8ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
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

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> v, int lane, int x) => v.AsInt16().WithElement(lane, (short)(x & 0xFFFF)).AsByte();
}

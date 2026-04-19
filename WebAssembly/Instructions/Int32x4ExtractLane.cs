using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtractLane instruction.</summary>
public class Int32x4ExtractLane : SimdExtractLaneInstruction, IEquatable<Int32x4ExtractLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtractLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtractLane;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ExtractLaneMethod;

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
}

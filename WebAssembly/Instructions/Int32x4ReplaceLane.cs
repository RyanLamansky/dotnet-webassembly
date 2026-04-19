using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ReplaceLane instruction.</summary>
public class Int32x4ReplaceLane : SimdReplaceLaneInstruction, IEquatable<Int32x4ReplaceLane>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ReplaceLane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ReplaceLane;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ReplaceLaneMethod;

    /// <summary>Creates a new <see cref="Int32x4ReplaceLane"/> instance.</summary>
    public Int32x4ReplaceLane() { }
    internal Int32x4ReplaceLane(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int32x4ReplaceLane);
    /// <inheritdoc/>
    public bool Equals(Int32x4ReplaceLane? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int32x4ReplaceLane);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

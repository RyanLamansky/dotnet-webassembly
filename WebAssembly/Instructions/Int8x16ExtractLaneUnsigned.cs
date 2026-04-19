using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ExtractLaneUnsigned instruction.</summary>
public class Int8x16ExtractLaneUnsigned : SimdExtractLaneInstruction, IEquatable<Int8x16ExtractLaneUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ExtractLaneUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ExtractLaneUnsigned;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ExtractLaneUMethod;

    /// <summary>Creates a new <see cref="Int8x16ExtractLaneUnsigned"/> instance.</summary>
    public Int8x16ExtractLaneUnsigned() { }
    internal Int8x16ExtractLaneUnsigned(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int8x16ExtractLaneUnsigned);
    /// <inheritdoc/>
    public bool Equals(Int8x16ExtractLaneUnsigned? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int8x16ExtractLaneUnsigned);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

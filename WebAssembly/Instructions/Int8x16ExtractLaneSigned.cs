using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16ExtractLaneSigned instruction.</summary>
public class Int8x16ExtractLaneSigned : SimdExtractLaneInstruction, IEquatable<Int8x16ExtractLaneSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ExtractLaneSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ExtractLaneSigned;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16ExtractLaneSMethod;

    /// <summary>Creates a new <see cref="Int8x16ExtractLaneSigned"/> instance.</summary>
    public Int8x16ExtractLaneSigned() { }
    internal Int8x16ExtractLaneSigned(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int8x16ExtractLaneSigned);
    /// <inheritdoc/>
    public bool Equals(Int8x16ExtractLaneSigned? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int8x16ExtractLaneSigned);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();
}

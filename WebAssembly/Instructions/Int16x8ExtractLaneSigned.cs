using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ExtractLaneSigned instruction.</summary>
public class Int16x8ExtractLaneSigned : SimdExtractLaneInstruction, IEquatable<Int16x8ExtractLaneSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtractLaneSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtractLaneSigned;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override byte MaxLaneCount => 8;

    /// <summary>Creates a new <see cref="Int16x8ExtractLaneSigned"/> instance.</summary>
    public Int16x8ExtractLaneSigned() { }
    internal Int16x8ExtractLaneSigned(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int16x8ExtractLaneSigned);
    /// <inheritdoc/>
    public bool Equals(Int16x8ExtractLaneSigned? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int16x8ExtractLaneSigned);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> v, int lane) => v.AsInt16().GetElement(lane);
}

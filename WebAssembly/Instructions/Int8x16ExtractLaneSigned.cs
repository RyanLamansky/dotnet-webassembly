using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Extract a signed i8 lane as i32.</summary>
public class Int8x16ExtractLaneSigned : SimdExtractLaneInstruction, IEquatable<Int8x16ExtractLaneSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16ExtractLaneSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16ExtractLaneSigned;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override byte MaxLaneCount => 16;

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

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> v, int lane) => (sbyte)v.GetElement(lane);
}

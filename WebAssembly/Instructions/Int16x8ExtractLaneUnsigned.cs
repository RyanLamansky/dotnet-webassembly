using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Extract an unsigned i16 lane as i32.</summary>
public class Int16x8ExtractLaneUnsigned : SimdExtractLaneInstruction, IEquatable<Int16x8ExtractLaneUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtractLaneUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtractLaneUnsigned;
    internal override WebAssemblyValueType ResultType => WebAssemblyValueType.Int32;
    internal override byte MaxLaneCount => 8;

    /// <summary>Creates a new <see cref="Int16x8ExtractLaneUnsigned"/> instance.</summary>
    public Int16x8ExtractLaneUnsigned() { }
    internal Int16x8ExtractLaneUnsigned(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int16x8ExtractLaneUnsigned);
    /// <inheritdoc/>
    public bool Equals(Int16x8ExtractLaneUnsigned? other) => other != null && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int16x8ExtractLaneUnsigned);
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> v, int lane) => v.AsUInt16().GetElement(lane);
}

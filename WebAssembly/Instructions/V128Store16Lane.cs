using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Store the specified i16x8 lane to memory.</summary>
public class V128Store16Lane : SimdInstruction, IEquatable<V128Store16Lane>
{
    /// <summary>Always <see cref="SimdOpCode.V128Store16Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Store16Lane;

    /// <summary>Alignment flags.</summary>
    public uint Flags { get; set; }
    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }
    /// <summary>Lane index (0-7).</summary>
    public byte LaneIndex { get; set; }

    /// <summary>Creates a new <see cref="V128Store16Lane"/> instance.</summary>
    public V128Store16Lane() { }

    internal V128Store16Lane(Reader reader)
    {
        Flags = reader.ReadVarUInt32();
        Offset = reader.ReadVarUInt32();
        LaneIndex = reader.ReadByte();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(Flags);
        writer.WriteVar(Offset);
        writer.Write(LaneIndex);
    }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.Int32);

        var vecLocal = context.DeclareLocal(V128Helper.V128Type);
        context.Emit(OpCodes.Stloc, vecLocal);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck16, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, V128Helper.V128Store16LaneMethod.Reference);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as V128Store16Lane);
    /// <inheritdoc/>
    public bool Equals(V128Store16Lane? other) => other != null && other.Flags == Flags && other.Offset == Offset && other.LaneIndex == LaneIndex;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as V128Store16Lane);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(HashCode.Combine((int)SimdOpCode, (int)Flags, (int)Offset), (int)LaneIndex);
}

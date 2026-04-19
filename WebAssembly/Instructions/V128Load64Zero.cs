using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 64 bits from memory into the low lane of a v128, zeroing the upper 64 bits.</summary>
public class V128Load64Zero : SimdInstruction, IEquatable<V128Load64Zero>
{
    /// <summary>Always <see cref="SimdOpCode.V128Load64Zero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load64Zero;

    /// <summary>Alignment flags.</summary>
    public uint Flags { get; set; }
    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }

    /// <summary>Creates a new <see cref="V128Load64Zero"/> instance.</summary>
    public V128Load64Zero() { }

    internal V128Load64Zero(Reader reader)
    {
        Flags = reader.ReadVarUInt32();
        Offset = reader.ReadVarUInt32();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(Flags);
        writer.WriteVar(Offset);
    }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck64, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Call, V128Helper.V128Load64ZeroMethod.Reference);

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as V128Load64Zero);
    /// <inheritdoc/>
    public bool Equals(V128Load64Zero? other) => other != null && other.Flags == Flags && other.Offset == Offset;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as V128Load64Zero);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)SimdOpCode, (int)Flags, (int)Offset);
}

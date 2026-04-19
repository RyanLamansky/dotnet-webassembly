using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 32 bits from memory into the low lane of a v128, zeroing the upper 96 bits.</summary>
public class V128Load32Zero : SimdInstruction, IEquatable<V128Load32Zero>
{
    /// <summary>Always <see cref="SimdOpCode.V128Load32Zero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load32Zero;

    /// <summary>Alignment flags.</summary>
    public uint Flags { get; set; }
    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }

    /// <summary>Creates a new <see cref="V128Load32Zero"/> instance.</summary>
    public V128Load32Zero() { }

    internal V128Load32Zero(Reader reader)
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
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck32, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Call, V128Helper.V128Load32ZeroMethod.Reference);

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as V128Load32Zero);
    /// <inheritdoc/>
    public bool Equals(V128Load32Zero? other) => other != null && other.Flags == Flags && other.Offset == Offset;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as V128Load32Zero);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)SimdOpCode, (int)Flags, (int)Offset);
}

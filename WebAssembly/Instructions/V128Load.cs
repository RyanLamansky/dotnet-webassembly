using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Load 16 bytes from memory as a v128 value.
/// </summary>
public class V128Load : SimdInstruction, IEquatable<V128Load>
{
    /// <summary>Always <see cref="SimdOpCode.V128Load"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load;

    /// <summary>Alignment flags (log2 of byte alignment).</summary>
    public uint Flags { get; set; }

    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }

    /// <summary>Creates a new <see cref="V128Load"/> instance.</summary>
    public V128Load() { }

    internal V128Load(Reader reader)
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
        if (this.Flags > 4)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck128, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Call, V128Helper.ReadUnalignedMethod.Reference);

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as V128Load);

    /// <inheritdoc/>
    public bool Equals(V128Load? other) =>
        other != null && other.Flags == this.Flags && other.Offset == this.Offset;

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as V128Load);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)SimdOpCode, (int)Flags, (int)Offset);
}

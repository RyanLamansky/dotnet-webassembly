using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load a 32-bit value and splat to all 4 lanes of a v128.</summary>
public class V128Load32Splat : SimdInstruction, IEquatable<V128Load32Splat>
{
    /// <summary>Always <see cref="SimdOpCode.V128Load32Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load32Splat;

    /// <summary>Alignment flags (log2 of byte alignment).</summary>
    public uint Flags { get; set; }

    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }

    /// <summary>Creates a new <see cref="V128Load32Splat"/> instance.</summary>
    public V128Load32Splat() { }

    internal V128Load32Splat(Reader reader)
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
        if (this.Flags > 2)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
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

        context.Emit(OpCodes.Call, V128Helper.V128Load32SplatMethod.Reference);

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as V128Load32Splat);
    /// <inheritdoc/>
    public bool Equals(V128Load32Splat? other) => other != null && other.Flags == this.Flags && other.Offset == this.Offset;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as V128Load32Splat);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)SimdOpCode, (int)Flags, (int)Offset);
}

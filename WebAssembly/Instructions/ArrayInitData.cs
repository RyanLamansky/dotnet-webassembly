using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from a passive data segment into an existing array, reinterpreting the segment bytes at the element width
/// (WebAssembly 3.0 garbage collection).  Traps on a null reference or an out-of-range read.
/// Stack: [(ref null $t), index:i32, offset:i32, count:i32] → []
/// </summary>
public class ArrayInitData : GCInstruction
{
    private static readonly MethodInfo initArrayFromDataMethod = typeof(WasmGC).GetMethod(nameof(WasmGC.InitArrayFromData))!;

    /// <summary>Always <see cref="GCOpCode.ArrayInitData"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayInitData;

    /// <summary>The array type index.</summary>
    public uint Type { get; set; }

    /// <summary>The data segment index.</summary>
    public uint Data { get; set; }

    /// <summary>Creates a new <see cref="ArrayInitData"/> instance.</summary>
    public ArrayInitData() { }

    internal ArrayInitData(Reader reader)
    {
        this.Type = reader.ReadVarUInt32();
        this.Data = reader.ReadVarUInt32();
    }

    private protected sealed override void WriteImmediates(Writer writer)
    {
        writer.WriteVar(this.Type);
        writer.WriteVar(this.Data);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayInitData instruction && instruction.Type == this.Type && instruction.Data == this.Data;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type, (int)this.Data);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        if (element.Packed == PackedType.None && element.ValueType.IsReference)
            throw new ModuleLoadException($"array type is not numeric or vector: array.init_data requires a numeric or vector element (type {this.Type}).", 0);
        if (!element.IsMutable)
            throw new ModuleLoadException($"array is immutable: array type {this.Type} cannot be written by array.init_data.", 0);
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        if (!context.DataSegments.TryGetValue(this.Data, out var segmentField))
            throw new ModuleLoadException($"unknown data segment {this.Data}: array.init_data requires a passive data segment.", 0);

        // Stack (bottom→top): array, index, offset, count.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // count
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // offset
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var count = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, count);
        var offset = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, offset);
        var index = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, index);

        // array on top: InitArrayFromData(items, index, data, offset, count).
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField);
        context.Emit(OpCodes.Ldloc, index);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segmentField);
        context.Emit(OpCodes.Ldloc, offset);
        context.Emit(OpCodes.Ldloc, count);
        context.Emit(OpCodes.Call, initArrayFromDataMethod.MakeGenericMethod(elementClrType));
    }
}

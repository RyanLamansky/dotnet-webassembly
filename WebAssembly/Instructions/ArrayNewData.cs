using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate an array initialized from a passive data segment, reinterpreting the segment bytes at the element
/// width (WebAssembly 3.0 garbage collection).  Traps on an out-of-range read.  Stack: [offset:i32, size:i32] → [(ref $t)]
/// </summary>
public class ArrayNewData : GCInstruction
{
    private static readonly MethodInfo newArrayFromDataMethod = typeof(WasmGC).GetMethod(nameof(WasmGC.NewArrayFromData))!;

    /// <summary>Always <see cref="GCOpCode.ArrayNewData"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayNewData;

    /// <summary>The array type index.</summary>
    public uint Type { get; set; }

    /// <summary>The data segment index.</summary>
    public uint Data { get; set; }

    /// <summary>Creates a new <see cref="ArrayNewData"/> instance.</summary>
    public ArrayNewData() { }

    internal ArrayNewData(Reader reader)
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
    public override bool Equals(Instruction? other) => other is ArrayNewData instruction && instruction.Type == this.Type && instruction.Data == this.Data;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type, (int)this.Data);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        if (element.Packed == PackedType.None && element.ValueType.IsReference)
            throw new ModuleLoadException($"array type is not numeric or vector: array.new_data requires a numeric or vector element (type {this.Type}).", 0);
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        if (!context.DataSegments.TryGetValue(this.Data, out var segmentField))
            throw new ModuleLoadException($"unknown data segment {this.Data}: array.new_data requires a passive data segment.", 0);

        // Stack (bottom→top): offset, size.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // size
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // offset
        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        var size = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, size);
        var offset = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, offset);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segmentField);
        context.Emit(OpCodes.Ldloc, offset);
        context.Emit(OpCodes.Ldloc, size);
        context.Emit(OpCodes.Call, newArrayFromDataMethod.MakeGenericMethod(elementClrType));
        var items = context.DeclareLocal(array.ItemsField.FieldType);
        context.Emit(OpCodes.Stloc, items);

        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(this.Type));
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Ldloc, items);
        context.Emit(OpCodes.Stfld, array.ItemsField);
    }
}

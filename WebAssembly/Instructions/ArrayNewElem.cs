using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate an array initialized from a passive element segment (WebAssembly 3.0 garbage collection).  Traps on an
/// out-of-range read.  Stack: [offset:i32, size:i32] → [(ref $t)]
/// </summary>
public class ArrayNewElem : GCInstruction
{
    private static readonly MethodInfo newArrayFromElementMethod = typeof(WasmGC).GetMethod(nameof(WasmGC.NewArrayFromElement))!;
    private static readonly MethodInfo getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle")!;

    /// <summary>Always <see cref="GCOpCode.ArrayNewElem"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayNewElem;

    /// <summary>The array type index.</summary>
    public uint Type { get; set; }

    /// <summary>The element segment index.</summary>
    public uint Element { get; set; }

    /// <summary>Creates a new <see cref="ArrayNewElem"/> instance.</summary>
    public ArrayNewElem() { }

    internal ArrayNewElem(Reader reader)
    {
        this.Type = reader.ReadVarUInt32();
        this.Element = reader.ReadVarUInt32();
    }

    private protected sealed override void WriteImmediates(Writer writer)
    {
        writer.WriteVar(this.Type);
        writer.WriteVar(this.Element);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayNewElem instruction && instruction.Type == this.Type && instruction.Element == this.Element;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type, (int)this.Element);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        if (element.Packed != PackedType.None || !element.ValueType.IsReference)
            throw new ModuleLoadException($"type mismatch: array.new_elem requires a reference element (type {this.Type}).", 0);
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        if (!context.ElementSegments.TryGetValue(this.Element, out var segmentField)
            || !context.ElementSegmentTypes.TryGetValue(this.Element, out var segmentElementType))
            throw new ModuleLoadException($"unknown elem segment {this.Element}: array.new_elem requires an element segment.", 0);
        if (!context.IsSubtype(segmentElementType, element.ValueType))
            throw new ModuleLoadException($"type mismatch: element segment {this.Element} ({segmentElementType}) is not assignable to array type {this.Type} element ({element.ValueType}).", 0);

        // Stack (bottom→top): offset, size.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // size
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // offset
        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        var size = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, size);
        var offset = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, offset);

        context.Emit(OpCodes.Ldtoken, elementClrType);
        context.Emit(OpCodes.Call, getTypeFromHandleMethod);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segmentField);
        context.Emit(OpCodes.Ldloc, offset);
        context.Emit(OpCodes.Ldloc, size);
        context.Emit(OpCodes.Call, newArrayFromElementMethod);
        context.Emit(OpCodes.Castclass, array.ItemsField.FieldType);
        var items = context.DeclareLocal(array.ItemsField.FieldType);
        context.Emit(OpCodes.Stloc, items);

        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(this.Type));
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Ldloc, items);
        context.Emit(OpCodes.Stfld, array.ItemsField);
    }
}

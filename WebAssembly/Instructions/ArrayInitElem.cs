using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from a passive element segment into an existing array (WebAssembly 3.0 garbage collection).  Traps on a
/// null reference or an out-of-range read.  Stack: [(ref null $t), index:i32, offset:i32, count:i32] → []
/// </summary>
public class ArrayInitElem : GCInstruction
{
    private static readonly MethodInfo initArrayFromElementMethod = typeof(WebAssemblyGC).GetMethod(nameof(WebAssemblyGC.InitArrayFromElement))!;

    /// <summary>Always <see cref="GCOpCode.ArrayInitElem"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayInitElem;

    /// <summary>The array type index.</summary>
    public uint Type { get; set; }

    /// <summary>The element segment index.</summary>
    public uint Element { get; set; }

    /// <summary>Creates a new <see cref="ArrayInitElem"/> instance.</summary>
    public ArrayInitElem() { }

    internal ArrayInitElem(Reader reader)
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
    public override bool Equals(Instruction? other) => other is ArrayInitElem instruction && instruction.Type == this.Type && instruction.Element == this.Element;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type, (int)this.Element);

    internal sealed override void Compile(CompilationContext context)
    {
        var (emitter, array, element, _) = GCEmit.ResolveArray(context, this.Type);
        if (element.Packed != PackedType.None || !element.ValueType.IsReference)
            throw new ModuleLoadException($"type mismatch: array.init_elem requires a reference element (type {this.Type}).", 0);
        if (!element.IsMutable)
            throw new ModuleLoadException($"array is immutable: array type {this.Type} cannot be written by array.init_elem.", 0);

        if (!context.ElementSegments.TryGetValue(this.Element, out var segmentField)
            || !context.ElementSegmentTypes.TryGetValue(this.Element, out var segmentElementType))
            throw new ModuleLoadException($"unknown elem segment {this.Element}: array.init_elem requires an element segment.", 0);
        if (!context.IsSubtype(segmentElementType, element.ValueType))
            throw new ModuleLoadException($"type mismatch: element segment {this.Element} ({segmentElementType}) is not assignable to array type {this.Type} element ({element.ValueType}).", 0);

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

        // array on top: InitArrayFromElement(items, index, segment, offset, count).
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField!);
        context.Emit(OpCodes.Ldloc, index);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segmentField);
        context.Emit(OpCodes.Ldloc, offset);
        context.Emit(OpCodes.Ldloc, count);
        context.Emit(OpCodes.Call, initArrayFromElementMethod);
    }
}

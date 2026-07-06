using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy a range of elements between two array instances (WebAssembly 3.0 garbage collection), handling overlap
/// safely.  Traps on a null reference or an out-of-range range.
/// Stack: [(ref null $dst), destIndex:i32, (ref null $src), srcIndex:i32, count:i32] → []
/// </summary>
public class ArrayCopy : GCInstruction
{
    private static readonly MethodInfo arrayCopyMethod = typeof(WebAssemblyGC).GetMethod(nameof(WebAssemblyGC.ArrayCopy))!;

    /// <summary>Always <see cref="GCOpCode.ArrayCopy"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayCopy;

    /// <summary>The destination array type index.</summary>
    public uint DestinationType { get; set; }

    /// <summary>The source array type index.</summary>
    public uint SourceType { get; set; }

    /// <summary>Creates a new <see cref="ArrayCopy"/> instance.</summary>
    public ArrayCopy() { }

    internal ArrayCopy(Reader reader)
    {
        this.DestinationType = reader.ReadVarUInt32();
        this.SourceType = reader.ReadVarUInt32();
    }

    private protected sealed override void WriteImmediates(Writer writer)
    {
        writer.WriteVar(this.DestinationType);
        writer.WriteVar(this.SourceType);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is ArrayCopy instruction && instruction.DestinationType == this.DestinationType && instruction.SourceType == this.SourceType;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.DestinationType, (int)this.SourceType);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.DestinationType}: no array types are defined.", 0);
        emitter.RequireArray(this.DestinationType);
        emitter.RequireArray(this.SourceType);
        var destination = emitter.GetArray(this.DestinationType);
        var source = emitter.GetArray(this.SourceType);
        var destinationElement = destination.Element!;
        var sourceElement = source.Element!;

        if (!destinationElement.IsMutable)
            throw new ModuleLoadException($"array is immutable: destination array type {this.DestinationType} cannot be written by array.copy.", 0);
        if (!IsElementAssignable(context, sourceElement, destinationElement))
            throw new ModuleLoadException($"array types do not match: source array type {this.SourceType} element is not assignable to destination array type {this.DestinationType}.", 0);

        // Stack (bottom→top): destination, destIndex, source, srcIndex, count.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // count
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // srcIndex
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.SourceType)); // source
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // destIndex
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.DestinationType)); // destination

        var count = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, count);
        var sourceIndex = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, sourceIndex);
        var sourceReference = context.DeclareLocal(typeof(object));
        context.Emit(OpCodes.Stloc, sourceReference);
        var destinationIndex = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, destinationIndex);
        var destinationReference = context.DeclareLocal(typeof(object));
        context.Emit(OpCodes.Stloc, destinationReference);

        // WebAssemblyGC.ArrayCopy(destItems, destIndex, srcItems, srcIndex, count).
        context.Emit(OpCodes.Ldloc, destinationReference);
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.DestinationType));
        context.Emit(OpCodes.Ldfld, destination.ItemsField!);
        context.Emit(OpCodes.Ldloc, destinationIndex);
        context.Emit(OpCodes.Ldloc, sourceReference);
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.SourceType));
        context.Emit(OpCodes.Ldfld, source.ItemsField!);
        context.Emit(OpCodes.Ldloc, sourceIndex);
        context.Emit(OpCodes.Ldloc, count);
        context.Emit(OpCodes.Call, arrayCopyMethod);
    }

    // The source element must be assignable to the destination element: packedness must match exactly; numeric
    // storage types must be identical; reference storage types are covariant (source <: destination).
    private static bool IsElementAssignable(CompilationContext context, GCTypeEmitter.GCField source, GCTypeEmitter.GCField destination)
    {
        if (source.Packed != destination.Packed)
            return false;
        if (source.Packed != PackedType.None)
            return true;

        var sourceType = source.ValueType;
        var destinationType = destination.ValueType;
        if (sourceType.IsReference != destinationType.IsReference)
            return false;
        return sourceType.IsReference
            ? context.IsSubtype(sourceType, destinationType)
            : sourceType.Numeric == destinationType.Numeric;
    }
}

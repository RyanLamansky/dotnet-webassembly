using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Write a mutable field of a struct: pop a value and a struct reference (WebAssembly 3.0 garbage collection).
/// Traps on a null reference.
/// </summary>
public class StructSet : StructFieldInstruction
{
    /// <summary>Always <see cref="GCOpCode.StructSet"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.StructSet;

    /// <summary>Creates a new <see cref="StructSet"/> instance.</summary>
    public StructSet() { }

    internal StructSet(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is StructSet i && i.Type == this.Type && i.Field == this.Field;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type, (int)this.Field);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no struct types are defined.", 0);
        var field = emitter.ResolveStructField(this.Type, this.Field);
        if (!field.IsMutable)
            throw new ModuleLoadException($"field is immutable: field {this.Field} of struct type {this.Type} cannot be written by struct.set.", 0);

        // Stack: [ref, value] with value on top.
        context.PopStackNoReturn(this.OpCode, field.StackType);
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type));

        // The struct reference is below the value; spill the value, cast the reference, restore the value, store.
        var value = context.DeclareLocal(field.Builder.FieldType);
        if (field.Packed == PackedType.I8)
            context.Emit(OpCodes.Conv_U1);
        else if (field.Packed == PackedType.I16)
            context.Emit(OpCodes.Conv_U2);
        context.Emit(OpCodes.Stloc, value);
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Stfld, field.Builder);
    }
}

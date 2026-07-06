using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate an array of a given size with every element set to its default value, and push a reference to it
/// (WebAssembly 3.0 garbage collection).  Valid only when the element is defaultable.  Stack: [size:i32] → [(ref $t)]
/// </summary>
public class ArrayNewDefault : ArrayInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArrayNewDefault"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayNewDefault;

    /// <summary>Creates a new <see cref="ArrayNewDefault"/> instance.</summary>
    public ArrayNewDefault() { }

    internal ArrayNewDefault(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayNewDefault instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        // A non-nullable reference element has no default value, so array.new_default is invalid for it.
        if (element.Packed == PackedType.None && element.ValueType.IsReference && !element.ValueType.IsNullable)
            throw new ModuleLoadException($"array.new_default: type {this.Type} has a non-defaultable (non-nullable reference) element.", 0);

        // Stack: size → (ref $t). CLR array allocation zero-initializes, matching the WASM defaults.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        var size = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, size);
        context.Emit(OpCodes.Ldloc, size);
        context.Emit(OpCodes.Newarr, elementClrType);
        var items = context.DeclareLocal(array.ItemsField.FieldType);
        context.Emit(OpCodes.Stloc, items);

        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(this.Type));
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Ldloc, items);
        context.Emit(OpCodes.Stfld, array.ItemsField);
    }
}

using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read a (non-packed) element of an array: pop an index and an array reference and push the element value
/// (WebAssembly 3.0 garbage collection).  Traps on a null reference or an out-of-range index.
/// </summary>
public class ArrayGet : ArrayInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArrayGet"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayGet;

    /// <summary>Creates a new <see cref="ArrayGet"/> instance.</summary>
    public ArrayGet() { }

    internal ArrayGet(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayGet instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        if (element.Packed != PackedType.None)
            throw new ModuleLoadException($"array.get on packed array type {this.Type} requires array.get_s or array.get_u.", 0);
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        // Stack (bottom→top): array, index.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var index = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, index);
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField);
        context.Emit(OpCodes.Ldloc, index);
        context.Emit(OpCodes.Ldelem, elementClrType);
        context.Stack.Push(element.ValueType);
    }
}

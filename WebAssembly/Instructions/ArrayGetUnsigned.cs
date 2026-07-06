using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read a packed (<c>i8</c>/<c>i16</c>) element of an array, zero-extending it to i32 (WebAssembly 3.0 garbage
/// collection).  Traps on a null reference or an out-of-range index.
/// </summary>
public class ArrayGetUnsigned : ArrayInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArrayGetUnsigned"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayGetUnsigned;

    /// <summary>Creates a new <see cref="ArrayGetUnsigned"/> instance.</summary>
    public ArrayGetUnsigned() { }

    internal ArrayGetUnsigned(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayGetUnsigned instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        if (element.Packed == PackedType.None)
            throw new ModuleLoadException($"array.get_u on non-packed array type {this.Type}; use array.get.", 0);
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var index = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, index);
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField);
        context.Emit(OpCodes.Ldloc, index);
        // The byte/ushort load is already zero-extended to i32.
        context.Emit(OpCodes.Ldelem, elementClrType);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}

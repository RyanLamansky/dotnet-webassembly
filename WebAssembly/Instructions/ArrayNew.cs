using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate an array of a given size, filling every element with an operand value, and push a reference to it
/// (WebAssembly 3.0 garbage collection).  Stack: [value, size:i32] → [(ref $t)]
/// </summary>
public class ArrayNew : ArrayInstruction
{
    private static readonly MethodInfo newArrayMethod = typeof(WasmGC).GetMethod(nameof(WasmGC.NewArray))!;

    /// <summary>Always <see cref="GCOpCode.ArrayNew"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayNew;

    /// <summary>Creates a new <see cref="ArrayNew"/> instance.</summary>
    public ArrayNew() { }

    internal ArrayNew(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayNew instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        // Stack (bottom→top): value, size.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // size
        context.PopStackNoReturn(this.OpCode, element.StackType); // value
        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        var size = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, size);
        var value = context.DeclareLocal(elementClrType);
        if (element.Packed == PackedType.I8)
            context.Emit(OpCodes.Conv_U1);
        else if (element.Packed == PackedType.I16)
            context.Emit(OpCodes.Conv_U2);
        context.Emit(OpCodes.Stloc, value);

        context.Emit(OpCodes.Ldloc, size);
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Call, newArrayMethod.MakeGenericMethod(elementClrType));
        var items = context.DeclareLocal(array.ItemsField.FieldType);
        context.Emit(OpCodes.Stloc, items);

        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(this.Type));
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Ldloc, items);
        context.Emit(OpCodes.Stfld, array.ItemsField);
    }
}

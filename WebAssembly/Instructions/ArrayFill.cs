using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Fill a range of a mutable array with an operand value (WebAssembly 3.0 garbage collection).  Traps on a null
/// reference or an out-of-range range.  Stack: [(ref null $t), index:i32, value, count:i32] → []
/// </summary>
public class ArrayFill : ArrayInstruction
{
    private static readonly MethodInfo fillArrayMethod = typeof(WasmGC).GetMethod(nameof(WasmGC.FillArray))!;

    /// <summary>Always <see cref="GCOpCode.ArrayFill"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayFill;

    /// <summary>Creates a new <see cref="ArrayFill"/> instance.</summary>
    public ArrayFill() { }

    internal ArrayFill(Reader reader) : base(reader) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayFill instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no array types are defined.", 0);
        emitter.RequireArray(this.Type);
        var array = emitter.GetArray(this.Type);
        var element = array.Element!;
        if (!element.IsMutable)
            throw new ModuleLoadException($"array is immutable: array type {this.Type} cannot be written by array.fill.", 0);
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;

        // Stack (bottom→top): array, index, value, count.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // count
        context.PopStackNoReturn(this.OpCode, element.StackType); // value
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var count = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, count);
        var value = context.DeclareLocal(elementClrType);
        if (element.Packed == PackedType.I8)
            context.Emit(OpCodes.Conv_U1);
        else if (element.Packed == PackedType.I16)
            context.Emit(OpCodes.Conv_U2);
        context.Emit(OpCodes.Stloc, value);
        var index = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, index);

        // array on top: cast, load Items, then (items, index, value, count).
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField);
        context.Emit(OpCodes.Ldloc, index);
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Ldloc, count);
        context.Emit(OpCodes.Call, fillArrayMethod.MakeGenericMethod(elementClrType));
    }
}

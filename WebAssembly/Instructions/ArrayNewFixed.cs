using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate an array from a fixed number of operands on the stack (last operand on top) and push a reference to it
/// (WebAssembly 3.0 garbage collection).  Stack: [v0, v1, …, v(size-1)] → [(ref $t)]
/// </summary>
public class ArrayNewFixed : GCInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArrayNewFixed"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayNewFixed;

    /// <summary>The array type index.</summary>
    public uint Type { get; set; }

    /// <summary>The number of operands popped to fill the array.</summary>
    public uint Size { get; set; }

    /// <summary>Creates a new <see cref="ArrayNewFixed"/> instance.</summary>
    public ArrayNewFixed() { }

    internal ArrayNewFixed(Reader reader)
    {
        this.Type = reader.ReadVarUInt32();
        this.Size = reader.ReadVarUInt32();
    }

    private protected sealed override void WriteImmediates(Writer writer)
    {
        writer.WriteVar(this.Type);
        writer.WriteVar(this.Size);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayNewFixed instruction && instruction.Type == this.Type && instruction.Size == this.Size;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type, (int)this.Size);

    internal sealed override void Compile(CompilationContext context)
    {
        var (emitter, array, element, elementClrType) = GCEmit.ResolveArray(context, this.Type);

        var count = checked((int)this.Size);

        // Operands are on the stack in element order (v0 deepest, last on top): pop last-first, spilling to locals.
        var locals = new LocalBuilder[count];
        for (var i = count - 1; i >= 0; i--)
            context.PopStackNoReturn(this.OpCode, element.StackType);
        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        for (var i = count - 1; i >= 0; i--)
        {
            var local = locals[i] = context.DeclareLocal(elementClrType);
            GCEmit.EmitPackedStoreConversion(context, element.Packed);
            context.Emit(OpCodes.Stloc, local);
        }

        context.Emit(OpCodes.Ldc_I4, count);
        context.Emit(OpCodes.Newarr, elementClrType);
        var items = context.DeclareLocal(array.ItemsField!.FieldType);
        context.Emit(OpCodes.Stloc, items);
        for (var i = 0; i < count; i++)
        {
            context.Emit(OpCodes.Ldloc, items);
            context.Emit(OpCodes.Ldc_I4, i);
            context.Emit(OpCodes.Ldloc, locals[i]);
            context.Emit(OpCodes.Stelem, elementClrType);
        }

        GCEmit.EmitWrapArrayObject(context, emitter, this.Type, array, items);
    }
}

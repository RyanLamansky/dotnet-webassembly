using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Write a mutable element of an array: pop a value, an index, and an array reference (WebAssembly 3.0 garbage
/// collection).  Traps on a null reference or an out-of-range index.
/// </summary>
public class ArraySet : ArrayInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArraySet"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArraySet;

    /// <summary>Creates a new <see cref="ArraySet"/> instance.</summary>
    public ArraySet() { }

    internal ArraySet(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var (emitter, array, element, elementClrType) = GCEmit.ResolveArray(context, this.Type);
        if (!element.IsMutable)
            throw new ModuleLoadException($"array is immutable: array type {this.Type} cannot be written by array.set.", 0);

        // Stack (bottom→top): array, index, value.
        context.PopStackNoReturn(this.OpCode, element.StackType); // value
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var value = context.DeclareLocal(elementClrType);
        GCEmit.EmitPackedStoreConversion(context, element.Packed);
        context.Emit(OpCodes.Stloc, value);
        var index = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, index);

        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField!);
        context.Emit(OpCodes.Ldloc, index);
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Stelem, elementClrType);
    }
}

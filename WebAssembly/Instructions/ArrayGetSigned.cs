using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read a packed (<c>i8</c>/<c>i16</c>) element of an array, sign-extending it to i32 (WebAssembly 3.0 garbage
/// collection).  Traps on a null reference or an out-of-range index.
/// </summary>
public class ArrayGetSigned : ArrayInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArrayGetSigned"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayGetSigned;

    /// <summary>Creates a new <see cref="ArrayGetSigned"/> instance.</summary>
    public ArrayGetSigned() { }

    internal ArrayGetSigned(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var (emitter, array, element, elementClrType) = GCEmit.ResolveArray(context, this.Type);
        if (element.Packed == PackedType.None)
            throw new ModuleLoadException($"array.get_s on non-packed array type {this.Type}; use array.get.", 0);

        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var index = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, index);
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField!);
        context.Emit(OpCodes.Ldloc, index);
        context.Emit(OpCodes.Ldelem, elementClrType);
        // The byte/ushort load is zero-extended; sign-extend from the packed width.
        context.Emit(element.Packed == PackedType.I8 ? OpCodes.Conv_I1 : OpCodes.Conv_I2);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}

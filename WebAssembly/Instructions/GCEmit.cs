using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Shared IL-emission snippets for the WebAssembly 3.0 array (and packed-field) instructions, factoring out the
/// resolve preamble, the element-array-to-object wrap tail, and the packed-store narrowing that otherwise repeat
/// across the array and struct instruction classes.
/// </summary>
static class GCEmit
{
    /// <summary>
    /// Resolves and validates an array type index, returning its emitter, emitted class metadata, element metadata,
    /// and CLR element type. Throws the standard "unknown type" / "not an array" module-load errors.
    /// </summary>
    public static (GCTypeEmitter Emitter, GCTypeEmitter.EmittedType Array, GCTypeEmitter.GCField Element, Type ElementClrType)
        ResolveArray(CompilationContext context, uint type)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {type}: no array types are defined.", 0);
        emitter.RequireArray(type);
        var array = emitter.GetArray(type);
        var element = array.Element!;
        var elementClrType = array.ItemsField!.FieldType.GetElementType()!;
        return (emitter, array, element, elementClrType);
    }

    /// <summary>
    /// Constructs the array object (default constructor) and stores the already-populated element array (held in
    /// <paramref name="items"/>) into its <c>Items</c> field, leaving the new object reference on the stack.
    /// </summary>
    public static void EmitWrapArrayObject(CompilationContext context, GCTypeEmitter emitter, uint type, GCTypeEmitter.EmittedType array, LocalBuilder items)
    {
        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(type));
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Ldloc, items);
        context.Emit(OpCodes.Stfld, array.ItemsField!);
    }

    /// <summary>
    /// Narrows the value on top of the stack to a packed field/element's storage width (<c>i8</c> → <c>conv.u1</c>,
    /// <c>i16</c> → <c>conv.u2</c>); a no-op for a non-packed field.
    /// </summary>
    public static void EmitPackedStoreConversion(CompilationContext context, PackedType packed)
    {
        if (packed == PackedType.I8)
            context.Emit(OpCodes.Conv_U1);
        else if (packed == PackedType.I16)
            context.Emit(OpCodes.Conv_U2);
    }
}

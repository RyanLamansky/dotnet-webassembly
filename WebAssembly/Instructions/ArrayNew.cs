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
    private static readonly MethodInfo newArrayMethod = typeof(WebAssemblyGC).GetMethod(nameof(WebAssemblyGC.NewArray))!;

    /// <summary>Always <see cref="GCOpCode.ArrayNew"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayNew;

    /// <summary>Creates a new <see cref="ArrayNew"/> instance.</summary>
    public ArrayNew() { }

    internal ArrayNew(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var (emitter, array, element, elementClrType) = GCEmit.ResolveArray(context, this.Type);

        // Stack (bottom→top): value, size.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // size
        context.PopStackNoReturn(this.OpCode, element.StackType); // value
        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        var size = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, size);
        var value = context.DeclareLocal(elementClrType);
        GCEmit.EmitPackedStoreConversion(context, element.Packed);
        context.Emit(OpCodes.Stloc, value);

        context.Emit(OpCodes.Ldloc, size);
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Call, newArrayMethod.MakeGenericMethod(elementClrType));
        var items = context.DeclareLocal(array.ItemsField!.FieldType);
        context.Emit(OpCodes.Stloc, items);

        GCEmit.EmitWrapArrayObject(context, emitter, this.Type, array, items);
    }
}

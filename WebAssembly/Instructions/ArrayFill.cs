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
    private static readonly MethodInfo fillArrayMethod = typeof(WebAssemblyGC).GetMethod(nameof(WebAssemblyGC.FillArray))!;

    /// <summary>Always <see cref="GCOpCode.ArrayFill"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayFill;

    /// <summary>Creates a new <see cref="ArrayFill"/> instance.</summary>
    public ArrayFill() { }

    internal ArrayFill(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var (emitter, array, element, elementClrType) = GCEmit.ResolveArray(context, this.Type);
        if (!element.IsMutable)
            throw new ModuleLoadException($"array is immutable: array type {this.Type} cannot be written by array.fill.", 0);

        // Stack (bottom→top): array, index, value, count.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // count
        context.PopStackNoReturn(this.OpCode, element.StackType); // value
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // index
        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type)); // array

        var count = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, count);
        var value = context.DeclareLocal(elementClrType);
        GCEmit.EmitPackedStoreConversion(context, element.Packed);
        context.Emit(OpCodes.Stloc, value);
        var index = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, index);

        // array on top: cast, load Items, then (items, index, value, count).
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, array.ItemsField!);
        context.Emit(OpCodes.Ldloc, index);
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Ldloc, count);
        context.Emit(OpCodes.Call, fillArrayMethod.MakeGenericMethod(elementClrType));
    }
}

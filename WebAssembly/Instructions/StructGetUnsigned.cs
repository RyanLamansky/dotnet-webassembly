using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read a packed (<c>i8</c>/<c>i16</c>) field of a struct, zero-extending it to i32 (WebAssembly 3.0 garbage
/// collection).  Traps on a null reference.
/// </summary>
public class StructGetUnsigned : StructFieldInstruction
{
    /// <summary>Always <see cref="GCOpCode.StructGetUnsigned"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.StructGetUnsigned;

    /// <summary>Creates a new <see cref="StructGetUnsigned"/> instance.</summary>
    public StructGetUnsigned() { }

    internal StructGetUnsigned(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no struct types are defined.", 0);
        var field = emitter.ResolveStructField(this.Type, this.Field);
        if (field.Packed == PackedType.None)
            throw new ModuleLoadException($"struct.get_u on non-packed field {this.Field} of type {this.Type}; use struct.get.", 0);

        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type));
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        // ldfld of the byte/ushort field already loads zero-extended to i32.
        context.Emit(OpCodes.Ldfld, field.Builder);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}

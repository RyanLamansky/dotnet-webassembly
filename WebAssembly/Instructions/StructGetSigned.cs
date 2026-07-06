using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read a packed (<c>i8</c>/<c>i16</c>) field of a struct, sign-extending it to i32 (WebAssembly 3.0 garbage
/// collection).  Traps on a null reference.
/// </summary>
public class StructGetSigned : StructFieldInstruction
{
    /// <summary>Always <see cref="GCOpCode.StructGetSigned"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.StructGetSigned;

    /// <summary>Creates a new <see cref="StructGetSigned"/> instance.</summary>
    public StructGetSigned() { }

    internal StructGetSigned(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no struct types are defined.", 0);
        var field = emitter.ResolveStructField(this.Type, this.Field);
        if (field.Packed == PackedType.None)
            throw new ModuleLoadException($"struct.get_s on non-packed field {this.Field} of type {this.Type}; use struct.get.", 0);

        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type));
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, field.Builder);
        // ldfld of the byte/ushort field loads zero-extended; sign-extend from the packed width.
        context.Emit(field.Packed == PackedType.I8 ? OpCodes.Conv_I1 : OpCodes.Conv_I2);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}

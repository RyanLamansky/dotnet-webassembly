using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read a (non-packed) field of a struct: pop a struct reference and push the field value (WebAssembly 3.0
/// garbage collection).  Traps on a null reference.
/// </summary>
public class StructGet : StructFieldInstruction
{
    /// <summary>Always <see cref="GCOpCode.StructGet"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.StructGet;

    /// <summary>Creates a new <see cref="StructGet"/> instance.</summary>
    public StructGet() { }

    internal StructGet(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no struct types are defined.", 0);
        var field = emitter.ResolveStructField(this.Type, this.Field);
        if (field.Packed != PackedType.None)
            throw new ModuleLoadException($"struct.get on packed field {this.Field} of type {this.Type} requires struct.get_s or struct.get_u.", 0);

        context.PopStackNoReturn(this.OpCode, ValType.RefIndexed(true, this.Type));
        context.Emit(OpCodes.Castclass, emitter.GetClass(this.Type));
        context.Emit(OpCodes.Ldfld, field.Builder);
        context.Stack.Push(field.ValueType);
    }
}

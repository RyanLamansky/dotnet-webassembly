using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate a struct with every field set to its default value and push a reference to it (WebAssembly 3.0
/// garbage collection).  Valid only when every field is defaultable (numeric, packed, or nullable reference).
/// </summary>
public class StructNewDefault : GCInstruction
{
    /// <summary>Always <see cref="GCOpCode.StructNewDefault"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.StructNewDefault;

    /// <summary>The type index of the struct to allocate.</summary>
    public uint Type { get; set; }

    /// <summary>Creates a new <see cref="StructNewDefault"/> instance.</summary>
    public StructNewDefault() { }

    internal StructNewDefault(Reader reader) => this.Type = reader.ReadVarUInt32();

    private protected sealed override void WriteImmediates(Writer writer) => writer.WriteVar(this.Type);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is StructNewDefault instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no struct types are defined.", 0);
        emitter.RequireStruct(this.Type);

        foreach (var field in emitter.GetFields(this.Type))
        {
            // A non-nullable reference field has no default value, so struct.new_default is invalid for it.
            if (field.Packed == PackedType.None && field.ValueType.IsReference && !field.ValueType.IsNullable)
                throw new ModuleLoadException($"struct.new_default: type {this.Type} has a non-defaultable (non-nullable reference) field.", 0);
        }

        context.Stack.Push(ValType.RefIndexed(false, this.Type));
        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(this.Type));
    }
}

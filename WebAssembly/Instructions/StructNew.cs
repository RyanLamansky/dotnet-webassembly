using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Allocate a struct, popping one operand per field (in field order, last field on top of the stack), and push a
/// reference to it (WebAssembly 3.0 garbage collection).
/// </summary>
public class StructNew : GCInstruction
{
    /// <summary>Always <see cref="GCOpCode.StructNew"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.StructNew;

    /// <summary>The type index of the struct to allocate.</summary>
    public uint Type { get; set; }

    /// <summary>Creates a new <see cref="StructNew"/> instance.</summary>
    public StructNew() { }

    internal StructNew(Reader reader) => this.Type = reader.ReadVarUInt32();

    private protected sealed override void WriteImmediates(Writer writer) => writer.WriteVar(this.Type);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is StructNew instruction && instruction.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.GCOpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        var emitter = context.GCTypes ?? throw new ModuleLoadException($"unknown type {this.Type}: no struct types are defined.", 0);
        emitter.RequireStruct(this.Type);
        var fields = emitter.GetFields(this.Type);

        // Operands are on the stack in field order (field 0 deepest, last field on top): pop last-first.
        for (var i = fields.Length - 1; i >= 0; i--)
            context.PopStackNoReturn(this.OpCode, fields[i].StackType);

        context.Stack.Push(ValType.RefIndexed(false, this.Type));

        // Spill operands (reverse) into locals, construct, then set each field. This keeps the stack-order → field
        // mapping correct without requiring a per-class field constructor.
        var locals = new LocalBuilder[fields.Length];
        for (var i = fields.Length - 1; i >= 0; i--)
        {
            var local = locals[i] = context.DeclareLocal(fields[i].Builder.FieldType);
            GCEmit.EmitPackedStoreConversion(context, fields[i].Packed);
            context.Emit(OpCodes.Stloc, local);
        }

        context.Emit(OpCodes.Newobj, emitter.GetDefaultConstructor(this.Type));
        for (var i = 0; i < fields.Length; i++)
        {
            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldloc, locals[i]);
            context.Emit(OpCodes.Stfld, fields[i].Builder);
        }
    }
}

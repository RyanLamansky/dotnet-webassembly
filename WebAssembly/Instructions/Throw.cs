using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Throws an exception carrying the operands of a given tag (WASM 3.0 exception handling).
/// </summary>
public class Throw : Instruction, IEquatable<Throw>
{
    /// <summary>
    /// Always <see cref="OpCode.Throw"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Throw;

    /// <summary>
    /// The index of the tag whose exception is thrown.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new <see cref="Throw"/> instance.
    /// </summary>
    public Throw()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Throw"/> instance for the given tag index.
    /// </summary>
    /// <param name="index">The index of the tag whose exception is thrown.</param>
    public Throw(uint index) => this.Index = index;

    internal Throw(Reader reader) => this.Index = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.Throw);
        writer.WriteVar(this.Index);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Throw);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Throw);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(Throw? other) => other != null && other.Index == this.Index;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.Throw, (int)this.Index);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {this.Index}";

    internal sealed override void Compile(CompilationContext context)
    {
        // Validate the tag index (throws "unknown tag N" when out of range) and pop its operands, subtype-checked in
        // reverse (top of stack is the last operand).
        var tagField = context.GetTag(this.Index);
        var signature = context.TagSignatures[(int)this.Index];
        var parameters = signature.RawParameterTypes;

        var unreachable = context.IsUnreachable;

        // In unreachable code this is validation only — the operands may not be physically present, so emit nothing.
        if (unreachable)
        {
            for (var i = parameters.Length - 1; i >= 0; i--)
                context.PopStackNoReturn(OpCode.Throw, parameters[i]);
            context.MarkUnreachable();
            return;
        }

        // Stash the operands into temporaries (reverse order, since the last operand is on top of the stack).
        var temps = new LocalBuilder[parameters.Length];
        for (var i = parameters.Length - 1; i >= 0; i--)
        {
            context.PopStackNoReturn(OpCode.Throw, parameters[i]);
            temps[i] = context.DeclareLocal(parameters[i].ToSystemType());
            context.Emit(OpCodes.Stloc, temps[i]);
        }

        // new WebAssemblyException(this.<tag>, new object[] { boxed operands… }); throw;
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, tagField);

        context.Emit(OpCodes.Ldc_I4, parameters.Length);
        context.Emit(OpCodes.Newarr, context.Configuration.NeutralizeType(typeof(object)));
        for (var i = 0; i < parameters.Length; i++)
        {
            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldc_I4, i);
            context.Emit(OpCodes.Ldloc, temps[i]);
            var clrType = parameters[i].ToSystemType();
            if (clrType.IsValueType)
                context.Emit(OpCodes.Box, context.Configuration.NeutralizeType(clrType));
            context.Emit(OpCodes.Stelem_Ref);
        }

        context.Emit(OpCodes.Newobj, context.Configuration.NeutralizeType(typeof(WebAssemblyException))
            .GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2));
        context.Emit(OpCodes.Throw);

        context.MarkUnreachable();
    }
}

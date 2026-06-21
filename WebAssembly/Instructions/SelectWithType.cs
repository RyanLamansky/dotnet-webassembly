using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// A typed ternary operator (WASM 2.0): like <see cref="Select"/> but carries an explicit value-type
/// annotation, which allows it to operate on reference types. Returns the first operand if the condition
/// operand is non-zero, or the second otherwise.
/// </summary>
public class SelectWithType : Instruction, IEquatable<SelectWithType>
{
    /// <summary>
    /// Always <see cref="OpCode.SelectWithType"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.SelectWithType;

    /// <summary>
    /// The declared type of the two operands.
    /// </summary>
    public WebAssemblyValueType Type { get; set; }

    /// <summary>
    /// Creates a new <see cref="SelectWithType"/> instance defaulting to <see cref="WebAssemblyValueType.Int32"/>.
    /// </summary>
    public SelectWithType()
    {
    }

    /// <summary>
    /// Creates a new <see cref="SelectWithType"/> instance for the provided type.
    /// </summary>
    /// <param name="type">The type of the two operands.</param>
    public SelectWithType(WebAssemblyValueType type) => this.Type = type;

    internal SelectWithType(Reader reader)
    {
        var count = reader.ReadVarUInt32();
        if (count != 1)
            throw new ModuleLoadException($"A typed select must carry exactly one type annotation, found {count}.", reader.Offset);

        this.Type = (WebAssemblyValueType)reader.ReadVarInt7();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        writer.WriteVar(1u); // The annotation vector always contains exactly one type.
        writer.WriteVar((sbyte)this.Type);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as SelectWithType);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as SelectWithType);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(SelectWithType? other) => other != null && other.Type == this.Type;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Type);

    internal sealed override void Compile(CompilationContext context)
    {
        // The operand type is carried in the immediate, so the stack effect is [T, T, i32] -> [T].
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        context.PopStackNoReturn(this.OpCode, this.Type);
        context.PopStackNoReturn(this.OpCode, this.Type);
        context.Stack.Push(this.Type);

        var helper = this.Type switch
        {
            WebAssemblyValueType.Int32 => HelperMethod.SelectInt32,
            WebAssemblyValueType.Int64 => HelperMethod.SelectInt64,
            WebAssemblyValueType.Float32 => HelperMethod.SelectFloat32,
            WebAssemblyValueType.Float64 => HelperMethod.SelectFloat64,
            WebAssemblyValueType.FuncRef or WebAssemblyValueType.ExternRef => HelperMethod.SelectObject,
            WebAssemblyValueType.V128 => HelperMethod.SelectV128,
            _ => throw new CompilerException($"Unsupported type for typed select: {this.Type}."),
        };
        context.Emit(OpCodes.Call, context[helper, Select.CreateSelectHelper]);

        // The object-typed helper returns System.Object; funcref values must be restored to System.Delegate
        // so that downstream consumers see the correct CLR type. externref already maps to System.Object.
        if (this.Type == WebAssemblyValueType.FuncRef)
            context.Emit(OpCodes.Castclass, typeof(Delegate));
    }
}

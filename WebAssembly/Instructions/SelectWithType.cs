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

    private WebAssemblyValueType type;
    private ReferenceType? refType;

    /// <summary>
    /// The declared type of the two operands. For a WASM 3.0 long-form typed reference this reports
    /// <see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/>;
    /// the heap-type detail is available via <see cref="RefType"/>. Setting this clears <see cref="RefType"/>.
    /// </summary>
    public WebAssemblyValueType Type
    {
        get => this.refType?.ToValueType() ?? this.type;
        set { this.type = value; this.refType = null; }
    }

    /// <summary>
    /// The full WASM 3.0 reference type when the operand type is a long-form typed reference, otherwise null.
    /// </summary>
    public ReferenceType? RefType
    {
        get => this.refType;
        set => this.refType = value;
    }

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

        this.type = ReferenceType.ReadValueType(reader, out this.refType);
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        writer.WriteVar(1u); // The annotation vector always contains exactly one type.
        if (this.refType is { } rt)
            rt.WriteTo(writer);
        else
            writer.WriteVar((sbyte)this.type);
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
    public bool Equals(SelectWithType? other) => other != null && other.Type == this.Type && other.refType == this.refType;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Type, this.refType?.GetHashCode() ?? 0);

    internal sealed override void Compile(CompilationContext context)
    {
        // The annotation may be a WASM 3.0 long-form typed reference; canonicalize it so subtype-matched
        // reference operands are accepted (the operands need only be subtypes of the annotation).
        if (this.refType is { TypeIndex: { } refTypeIndex })
        {
            var typeCount = context.Types?.Length ?? 0;
            if (refTypeIndex >= (uint)typeCount)
                throw new ModuleLoadException($"unknown type {refTypeIndex}: the module defines only {typeCount} type(s).", 0);
        }

        var valType = this.refType is { } rt ? ValType.FromReferenceType(rt) : ValType.FromValueType(this.type);

        // The operand type is carried in the immediate, so the stack effect is [T, T, i32] -> [T].
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        context.PopStackNoReturn(this.OpCode, valType);
        context.PopStackNoReturn(this.OpCode, valType);
        context.Stack.Push(valType);

        var systemType = context.SystemType(valType);
        HelperMethod helper;
        if (valType.IsReference)
            helper = HelperMethod.SelectObject; // Every reference (funcref/externref/typed) selects via the object helper.
        else
            helper = valType.Numeric switch
            {
                WebAssemblyValueType.Int32 => HelperMethod.SelectInt32,
                WebAssemblyValueType.Int64 => HelperMethod.SelectInt64,
                WebAssemblyValueType.Float32 => HelperMethod.SelectFloat32,
                WebAssemblyValueType.Float64 => HelperMethod.SelectFloat64,
                WebAssemblyValueType.V128 => HelperMethod.SelectV128,
                _ => throw new CompilerException($"Unsupported type for typed select: {this.Type}."),
            };
        context.Emit(OpCodes.Call, context[helper, Select.CreateSelectHelper]);

        // The object-typed helper returns System.Object; func-hierarchy values must be restored to
        // System.Delegate so downstream consumers see the correct CLR type. externref already maps to object.
        if (valType.IsReference && systemType == typeof(Delegate))
            context.Emit(OpCodes.Castclass, typeof(Delegate));
    }
}

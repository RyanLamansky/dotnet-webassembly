using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Typed select (select t*): like <see cref="Select"/> but with an explicit value-type annotation.
/// Stack: [T] [T] [i32] → [T]
/// </summary>
public class SelectWithType : Instruction
{
    /// <summary>Always <see cref="OpCode.SelectWithType"/>.</summary>
    public sealed override OpCode OpCode => OpCode.SelectWithType;

    /// <summary>The declared type of the two operands.</summary>
    public WebAssemblyValueType Type { get; set; }

    /// <summary>Creates a new <see cref="SelectWithType"/> instance.</summary>
    public SelectWithType() { }

    /// <summary>Creates a new <see cref="SelectWithType"/> for the given type.</summary>
    public SelectWithType(WebAssemblyValueType type) => Type = type;

    internal SelectWithType(Reader reader)
    {
        var count = reader.ReadVarUInt32();
        if (count != 1)
            throw new ModuleLoadException($"select t* must have exactly 1 type annotation, got {count}.", reader.Offset);
        Type = (WebAssemblyValueType)reader.ReadVarInt7();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.WriteVar(1u); // count always 1
        writer.WriteVar((sbyte)Type);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is SelectWithType s && s.Type == Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)Type);

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: T T i32 → T  (type is known from immediate)
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Int32);
        context.PopStackNoReturn(OpCode, Type);
        context.PopStackNoReturn(OpCode, Type);
        context.Stack.Push(Type);

        var helper = Type switch
        {
            WebAssemblyValueType.Int32 => HelperMethod.SelectInt32,
            WebAssemblyValueType.Int64 => HelperMethod.SelectInt64,
            WebAssemblyValueType.Float32 => HelperMethod.SelectFloat32,
            WebAssemblyValueType.Float64 => HelperMethod.SelectFloat64,
            WebAssemblyValueType.FuncRef or WebAssemblyValueType.ExternRef => HelperMethod.SelectObject,
            _ => throw new InvalidOperationException($"Unsupported type for select t*: {Type}"),
        };
        context.Emit(OpCodes.Call, context[helper, Select.CreateSelectHelper]);
    }
}

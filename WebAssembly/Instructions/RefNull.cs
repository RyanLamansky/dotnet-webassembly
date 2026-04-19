using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Produce a null reference of the given reference type.
/// </summary>
public class RefNull : Instruction, IEquatable<RefNull>
{
    /// <summary>
    /// Always <see cref="OpCode.RefNull"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.RefNull;

    /// <summary>
    /// The reference type of the null value (<see cref="WebAssemblyValueType.FuncRef"/> or <see cref="WebAssemblyValueType.ExternRef"/>).
    /// </summary>
    public WebAssemblyValueType Type { get; set; }

    /// <summary>
    /// Creates a new <see cref="RefNull"/> instance for funcref.
    /// </summary>
    public RefNull() => Type = WebAssemblyValueType.FuncRef;

    /// <summary>
    /// Creates a new <see cref="RefNull"/> instance with the given reference type.
    /// </summary>
    public RefNull(WebAssemblyValueType type) => Type = type;

    internal RefNull(Reader reader) => Type = (WebAssemblyValueType)reader.ReadVarInt7();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.RefNull);
        writer.WriteVar((sbyte)Type);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RefNull);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => Equals(other as RefNull);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(RefNull? other) => other != null && other.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.RefNull, (int)Type);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {Type}";

    internal sealed override void Compile(CompilationContext context)
    {
        context.Stack.Push(Type);
        context.Emit(OpCodes.Ldnull);
    }
}

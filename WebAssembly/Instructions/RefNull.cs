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
    /// Creates a new <see cref="RefNull"/> instance for <see cref="WebAssemblyValueType.FuncRef"/>.
    /// </summary>
    public RefNull() => this.Type = WebAssemblyValueType.FuncRef;

    /// <summary>
    /// Creates a new <see cref="RefNull"/> instance with the given reference type.
    /// </summary>
    public RefNull(WebAssemblyValueType type) => this.Type = type;

    internal RefNull(Reader reader) => this.Type = (WebAssemblyValueType)reader.ReadVarInt7();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.RefNull);
        writer.WriteVar((sbyte)this.Type);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as RefNull);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as RefNull);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(RefNull? other) => other != null && other.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.RefNull, (int)this.Type);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {this.Type}";

    internal sealed override void Compile(CompilationContext context)
    {
        context.Stack.Push(this.Type);
        context.Emit(OpCodes.Ldnull);
    }
}

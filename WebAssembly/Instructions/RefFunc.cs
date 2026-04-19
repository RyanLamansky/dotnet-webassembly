using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Produce a non-null reference to the function at the given index.
/// </summary>
public class RefFunc : Instruction, IEquatable<RefFunc>
{
    /// <summary>
    /// Always <see cref="OpCode.RefFunc"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.RefFunc;

    /// <summary>
    /// The index of the function to reference.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new <see cref="RefFunc"/> instance.
    /// </summary>
    public RefFunc()
    {
    }

    /// <summary>
    /// Creates a new <see cref="RefFunc"/> instance for the given function index.
    /// </summary>
    public RefFunc(uint index) => Index = index;

    internal RefFunc(Reader reader) => Index = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.RefFunc);
        writer.WriteVar(Index);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RefFunc);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => Equals(other as RefFunc);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(RefFunc? other) => other != null && other.Index == this.Index;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.RefFunc, (int)Index);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {Index}";

    internal sealed override void Compile(CompilationContext context)
    {
        // Pushes a funcref (represented as object) — emit a delegate load from the function table.
        // For now emit null as a placeholder; full implementation requires function-reference storage.
        context.Stack.Push(WebAssemblyValueType.FuncRef);
        context.Emit(OpCodes.Ldnull); // TODO: load actual function reference
    }
}

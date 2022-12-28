using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an throw.
/// </summary>
public class Throw : TagInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Throw"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Throw;

    /// <summary>
    /// Creates a new  <see cref="Throw"/> instance.
    /// </summary>
    public Throw()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Throw"/> instance.
    /// </summary>
    /// <param name="tagIndex">The index of the tag to throw.</param>
    public Throw(uint tagIndex)
        : base(tagIndex)
    {
    }

    internal Throw(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var tag = context.Tags?[this.Index] ?? throw new InvalidOperationException($"Tag {this.Index} not found.");
        var type = context.Types?[tag.TypeIndex] ?? throw new InvalidOperationException($"Type {tag.TypeIndex} not found.");

        if (tag == null)
            throw new InvalidOperationException($"Type index {this.Index} not found.");

        var exceptionType = type.ToException();
        var constructor = exceptionType.GetConstructors().Single();

        context.Emit(OpCodes.Ldc_I4, this.Index);
        context.Emit(OpCodes.Newobj, constructor);

        context.Emit(OpCodes.Throw);
        context.MarkUnreachable();
    }
}

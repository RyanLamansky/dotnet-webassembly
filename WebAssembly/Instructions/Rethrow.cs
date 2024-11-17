using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an Rethrow.
/// </summary>
public class Rethrow : TagInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Rethrow"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Rethrow;

    /// <summary>
    /// Creates a new  <see cref="Rethrow"/> instance.
    /// </summary>
    public Rethrow()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Throw"/> instance.
    /// </summary>
    /// <param name="relativeDepth">The relative depth of the exception to rethrow.</param>
    public Rethrow(uint relativeDepth)
        : base(relativeDepth)
    {
    }

    internal Rethrow(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.Rethrow();
        context.MarkUnreachable();
    }
}

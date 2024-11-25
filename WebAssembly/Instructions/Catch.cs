using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an catch.
/// </summary>
public class Catch : TagInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Catch"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Catch;

    /// <summary>
    /// Creates a new  <see cref="Catch"/> instance.
    /// </summary>
    public Catch()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Throw"/> instance.
    /// </summary>
    /// <param name="tagIndex">The index of the tag to throw.</param>
    public Catch(uint tagIndex)
        : base(tagIndex)
    {
    }

    internal Catch(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.BeginCatchBlock(Index);
        context.MarkReachable();
    }
}

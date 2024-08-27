using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an try.
/// </summary>
public class Try : BlockTypeInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Try"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Try;

    /// <summary>
    /// Creates a new  <see cref="Try"/> instance.
    /// </summary>
    public Try()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Try"/> of the provided type.
    /// </summary>
    /// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
    public Try(BlockType type)
        : base(type)
    {
    }

    internal Try(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var label = context.BeginExceptionBlock(Type);
        context.ExceptionLabels.Add(label);
        context.Labels.Add(checked((uint)context.Depth.Count), label);
        context.Depth.Push(this);
        context.BlockContexts.Add(context.Depth.Count, new BlockContext(context.Stack.Count));
    }
}

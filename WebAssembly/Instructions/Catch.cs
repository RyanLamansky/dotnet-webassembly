using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an catch.
/// </summary>
public class Catch : TagInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Catch"/>.
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
        var tag = context.Tags?[Index];

        if (tag is null)
        {
            throw new InvalidOperationException($"Tag {Index} not found.");
        }

        var type = context.Types?[tag.TypeIndex];

        if (type is null)
        {
            throw new InvalidOperationException($"Type {tag.TypeIndex} not found.");
        }

        var depth = checked((uint)context.Depth.Count - 1);
        var label = context.Labels[depth];

        if (!context.ExceptionLabels.Contains(label))
        {
            throw new InvalidOperationException("CatchAll must be inside a try block");
        }

        context.BeginCatchBlock(Index);
        context.MarkReachable();

        foreach (var parameterType in type.RawParameterTypes)
        {
            context.Stack.Push(parameterType);
        }
    }
}

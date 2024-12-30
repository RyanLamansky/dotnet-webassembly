using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an catch_all.
/// </summary>
public class CatchAll : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.CatchAll"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.CatchAll;

    /// <summary>
    /// Creates a new  <see cref="CatchAll"/> instance.
    /// </summary>
    public CatchAll()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var depth = checked((uint)context.Depth.Count - 1);
        var label = context.Labels[depth];

        if (!context.ExceptionLabels.Contains(label))
        {
            throw new InvalidOperationException("CatchAll must be inside a try block");
        }

        context.BeginCatchAllBlock();
        context.MarkReachable();
    }
}

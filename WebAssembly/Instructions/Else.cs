using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Marks the else block of an <see cref="If"/>.
/// </summary>
public class Else : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Else"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Else;

    /// <summary>
    /// Creates a new  <see cref="Else"/> instance.
    /// </summary>
    public Else()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var blockType = context.Depth.Count == 0 ? BlockType.Empty : context.Depth.Peek().Type;
        var blockContext = context.BlockContexts[context.Depth.Count];

        if (blockContext.BlockSignature != null)
        {
            // Multi-value if: the then-branch must have produced all result values; stash them so the else-branch
            // starts from the same baseline-plus-parameters state the then-branch did.
            var returns = blockContext.BlockSignature.RawReturnTypes;
            var expected = blockContext.InitialStackSize + returns.Length;
            if (!context.IsUnreachable && context.Stack.Count != expected)
                throw new StackSizeIncorrectException(OpCode.Else, expected, context.Stack.Count);

            if (!context.IsUnreachable && returns.Length > 0)
            {
                blockContext.ResultLocals ??= context.DeclareResultLocals(returns);
                for (var i = returns.Length - 1; i >= 0; i--)
                    context.Emit(OpCodes.Stloc, blockContext.ResultLocals[i]);
            }

            while (context.Stack.Count > blockContext.InitialStackSize)
                context.Stack.Pop();
            foreach (var parameter in blockContext.BlockSignature.RawParameterTypes)
                context.Stack.Push(parameter);
        }
        else if (blockType.TryToValueType(out var expectedType))
        {
            context.PopStackNoReturn(OpCode.Else, expectedType);
            if (!context.IsUnreachable)
                context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(0, expectedType));
        }

        // Jump over the else block to the construct's exit label (already registered as Labels[target] by If).
        var target = checked((uint)context.Depth.Count) - 1;
        context.Emit(OpCodes.Br, context.Labels[target]);

        // The false-branch (brfalse from If) lands here, at the start of the else block.
        context.MarkLabel(blockContext.IfFalseLabel!.Value);
        blockContext.IfFalseLabel = null;

        //Else-block is reachable even if the then-block is unreachable
        context.MarkReachable();
    }
}

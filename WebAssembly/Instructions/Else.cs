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
        var blockCtx = context.BlockContexts[context.Depth.Count];

        if (blockCtx.BlockSignature != null)
        {
            // Multi-value TypeIndex if-block: validate then-branch produced all result values.
            var returns = blockCtx.BlockSignature.RawReturnTypes;
            var expected = blockCtx.InitialStackSize + returns.Length;
            if (!context.IsUnreachable && context.Stack.Count != expected)
                throw new StackSizeIncorrectException(OpCode.Else, expected, context.Stack.Count);

            if (!context.IsUnreachable && returns.Length > 0)
            {
                // Stash then-branch results into ResultLocals.
                if (blockCtx.ResultLocals == null)
                {
                    blockCtx.ResultLocals = new LocalBuilder[returns.Length];
                    for (var i = 0; i < returns.Length; i++)
                        blockCtx.ResultLocals[i] = context.DeclareLocal(returns[i].ToSystemType());
                }
                for (var i = returns.Length - 1; i >= 0; i--)
                    context.Emit(OpCodes.Stloc, blockCtx.ResultLocals[i]);
            }

            // Reset tracking stack to InitialStackSize, then push params so else-branch starts with params available.
            while (context.Stack.Count > blockCtx.InitialStackSize)
                context.Stack.Pop();
            foreach (var pt in blockCtx.BlockSignature.RawParameterTypes)
                context.Stack.Push(pt);
        }
        else if (blockType.TryToValueType(out var expectedType))
        {
            context.PopStackNoReturn(OpCode.Else, expectedType);
            // Store then-block result before jumping over the else block.
            if (!context.IsUnreachable)
                context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(0, expectedType));
        }

        // Jump over the else block to the exit label (Labels[target] is already the exit label).
        var target = checked((uint)context.Depth.Count) - 1;
        context.Emit(OpCodes.Br, context.Labels[target]);

        // Mark where the false-branch (brfalse from If) lands.
        context.MarkLabel(blockCtx.IfFalseLabel!.Value);
        blockCtx.IfFalseLabel = null;

        //Else-block is reachable even if the then-block is unreachable
        context.MarkReachable();
    }
}

using System.Reflection.Emit;
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

        if (blockType.TryToValueType(out var expectedType))
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
        var blockCtx = context.BlockContexts[context.Depth.Count];
        context.MarkLabel(blockCtx.IfFalseLabel!.Value);
        blockCtx.IfFalseLabel = null;

        //Else-block is reachable even if the then-block is unreachable
        context.MarkReachable();
    }
}

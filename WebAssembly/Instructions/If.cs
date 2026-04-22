using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// The beginning of an if construct with an implicit "then" block.
/// </summary>
public class If : BlockTypeInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.If"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.If;

    /// <summary>
    /// Creates a new  <see cref="If"/> instance.
    /// </summary>
    public If()
    {
    }

    /// <summary>
    /// Creates a new <see cref="If"/> of the provided type.
    /// </summary>
    /// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
    public If(BlockType type)
        : base(type)
    {
    }

    internal If(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode.If, WebAssemblyValueType.Int32);

        // exitLabel: where br/end should jump (after the if-else construct).
        // falseLabel: where brfalse jumps (else-entry, or same as exitLabel if no else).
        var exitLabel = context.DefineLabel();
        var falseLabel = context.DefineLabel();
        context.Labels.Add(checked((uint)context.Depth.Count), exitLabel);
        context.Depth.Push(this);
        var blockCtx = new BlockContext(context.Stack.Count);
        blockCtx.IfFalseLabel = falseLabel;
        context.BlockContexts.Add(context.Depth.Count, blockCtx);
        context.Emit(OpCodes.Brfalse, falseLabel);
    }
}

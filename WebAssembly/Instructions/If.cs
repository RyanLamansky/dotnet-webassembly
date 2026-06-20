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

        // exitLabel is where the construct ends (the target of br/br_if to this depth and of the fall-through end).
        // falseLabel is where brfalse jumps when the condition is zero: the else entry, or — absent an else — the exit.
        var exitLabel = context.DefineLabel();
        var falseLabel = context.DefineLabel();
        context.Labels.Add(checked((uint)context.Depth.Count), exitLabel);
        var blockContext = BlockTypeInstruction.MakeBlockContext(this, context);
        blockContext.IfFalseLabel = falseLabel;
        context.Depth.Push(this);
        context.BlockContexts.Add(context.Depth.Count, blockContext);
        context.Emit(OpCodes.Brfalse, falseLabel);
    }
}

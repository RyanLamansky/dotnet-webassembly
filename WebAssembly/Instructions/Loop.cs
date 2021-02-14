using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// A block with a label at the beginning which may be used to form loops.
    /// </summary>
    public class Loop : BlockTypeInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Loop"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Loop;

        /// <summary>
        /// Creates a new  <see cref="Loop"/> instance.
        /// </summary>
        public Loop()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Loop"/> of the provided type.
        /// </summary>
        /// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
        public Loop(BlockType type)
            : base(type)
        {
        }

        internal Loop(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var loopStart = context.DefineLabel();
            context.Labels.Add(checked((uint)context.Depth.Count), loopStart);
            context.Depth.Push(this);
            context.BlockContexts.Add(context.Depth.Count, new BlockContext(context.Stack.Count));
            context.MarkLabel(loopStart);
            context.LoopLabels.Add(loopStart);
        }
    }
}
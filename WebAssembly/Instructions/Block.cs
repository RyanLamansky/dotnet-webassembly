using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// The beginning of a block construct, a sequence of instructions with a label at the end.
    /// </summary>
    public class Block : BlockTypeInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Block"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Block;

        /// <summary>
        /// Creates a new  <see cref="Block"/> instance.
        /// </summary>
        public Block()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Block"/> of the provided type.
        /// </summary>
        /// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
        public Block(BlockType type)
            : base(type)
        {
        }

        internal Block(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.Labels.Add(checked((uint)context.Depth.Count), context.DefineLabel());
            context.Depth.Push(this);
            context.BlockContexts.Add(context.Depth.Count, new BlockContext(context.Stack.Count));
        }
    }
}
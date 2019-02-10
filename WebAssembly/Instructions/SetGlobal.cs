using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// (i32 index, T value){T} => i32 index; Write a global variable.
    /// </summary>
    public class SetGlobal : VariableAccessInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.SetGlobal"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.SetGlobal;

        /// <summary>
        /// Creates a new  <see cref="SetGlobal"/> instance.
        /// </summary>
        public SetGlobal()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SetGlobal"/> for the provided variable index.
        /// </summary>
        /// <param name="index">The index of the variable to access.</param>
        public SetGlobal(uint index)
            : base(index)
        {
        }

        internal SetGlobal(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            if (context.Globals == null)
                throw new CompilerException("Can't use SetGlobal without a global section or global imports.");

            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(OpCode.SetGlobal, 1, stack.Count);

            Compile.GlobalInfo global;
            try
            {
                global = context.Globals[this.Index];
            }
            catch (System.IndexOutOfRangeException x)
            {
                throw new CompilerException($"Global at index {this.Index} does not exist.", x);
            }

            if (global.Setter == null)
                throw new CompilerException($"Global at index {this.Index} is immutable.");

            var type = stack.Pop();
            if (type != global.Type)
                throw new StackTypeInvalidException(OpCode.SetGlobal, global.Type, type);

            if (global.RequiresInstance)
                context.EmitLoadThis();

            context.Emit(OpCodes.Call, global.Setter);
        }
    }
}
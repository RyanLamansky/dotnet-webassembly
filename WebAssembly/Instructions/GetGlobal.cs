using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// (i32 index){T} => {T}; Read a global variable.
    /// </summary>
    public class GetGlobal : VariableAccessInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.GetGlobal"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.GetGlobal;

        /// <summary>
        /// Creates a new  <see cref="GetGlobal"/> instance.
        /// </summary>
        public GetGlobal()
        {
        }

        /// <summary>
        /// Creates a new <see cref="GetGlobal"/> for the provided variable index.
        /// </summary>
        /// <param name="index">The index of the variable to access.</param>
        public GetGlobal(uint index)
            : base(index)
        {
        }

        internal GetGlobal(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            if (context.Globals == null)
                throw new CompilerException("Can't use GetGlobal without a global section or global imports.");

            GlobalInfo global;
            try
            {
                global = context.Globals[this.Index];
            }
            catch (System.IndexOutOfRangeException x)
            {
                throw new CompilerException($"Global at index {this.Index} does not exist.", x);
            }

            if (global.RequiresInstance)
                context.EmitLoadThis();

            context.Emit(OpCodes.Call, global.Getter);

            context.Stack.Push(global.Type);
        }
    }
}
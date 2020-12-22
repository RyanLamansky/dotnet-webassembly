using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// An instruction which always traps.
    /// </summary>
    /// <remarks>It is intended to be used for example after calls to functions which are known by the producer not to return.</remarks>
    public class Unreachable : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Unreachable"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Unreachable;

        /// <summary>
        /// Creates a new  <see cref="Unreachable"/> instance.
        /// </summary>
        public Unreachable()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.Emit(OpCodes.Newobj, typeof(UnreachableException).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0));
            context.Emit(OpCodes.Throw);

            //Mark the subsequent code within this function is unreachable
            context.MarkUnreachable(functionWide: true);
        }
    }
}
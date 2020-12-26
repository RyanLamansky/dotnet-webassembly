using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Return zero or more values from this function.
    /// </summary>
    public class Return : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Return"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Return;

        /// <summary>
        /// Creates a new  <see cref="Return"/> instance.
        /// </summary>
        public Return()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var returns = context.CheckedSignature.RawReturnTypes;
            var stack = context.Stack;

            var returnsLength = returns.Length;
            Assert(returnsLength == 0 || returnsLength == 1); //WebAssembly doesn't currently offer multiple returns, which should be blocked earlier.

            var stackCount = stack.Count;

            if (stackCount > returnsLength)
            {
                if (returnsLength == 0)
                {
                    for (var i = 0; i < stackCount - returnsLength; i++)
                        context.Emit(OpCodes.Pop);
                }
                else
                {
                    var value = context.DeclareLocal(returns[0].ToSystemType());
                    context.Emit(OpCodes.Stloc, value.LocalIndex);

                    for (var i = 0; i < stackCount - returnsLength; i++)
                        context.Emit(OpCodes.Pop);

                    context.Emit(OpCodes.Ldloc, value.LocalIndex);
                }
            }

            if (returnsLength == 1)
                context.PopStackNoReturn(OpCode.Return, returns[0]);

            context.Emit(OpCodes.Ret);

            //Mark the subsequent code within this function is unreachable
            context.MarkUnreachable(functionWide: true);
        }
    }
}
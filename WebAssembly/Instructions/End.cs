using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// An instruction that marks the end of a block, loop, if, or function.
    /// </summary>
    public class End : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.End"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.End;

        /// <summary>
        /// Creates a new <see cref="End"/> instance.
        /// </summary>
        public End()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            var blockContext = context.BlockContexts[checked((uint)context.Depth.Count)];
            var blockType = context.Depth.Count == 0 ? BlockType.Empty : context.Depth.Peek();

            if (context.Depth.Count == 1)
            {
                if (context.Previous == OpCode.Return)
                    return; //WebAssembly requires functions to end on "end", but an immediately previous return is allowed.

                var returns = context.CheckedSignature.RawReturnTypes;
                var returnsLength = returns.Length;
                if (returnsLength < stack.Count || (returnsLength > stack.Count && !context.IsUnreachable()))
                    throw new StackSizeIncorrectException(OpCode.End, returnsLength, stack.Count);

                Assert(returnsLength == 0 || returnsLength == 1); //WebAssembly doesn't currently offer multiple returns, which should be blocked earlier.

                if (returnsLength == 1)
                {
                    var popped = context.PopStack(OpCode.End, returns[0]);
                    if (!popped[0].HasValue)
                        throw new OpCodeCompilationException(OpCode.End, "Cannot determine stack type.");
                }

                context.Emit(OpCodes.Ret);
            }
            else
            {
                if (blockType.TryToValueType(out var expectedType))
                {
                    var peeked = context.PeekStack(OpCode.End, expectedType);
                    if (!peeked[0].HasValue)
                        throw new OpCodeCompilationException(OpCode.End, "Cannot determine stack type.");
                }

                context.BlockContexts.Remove(checked((uint)context.Depth.Count));
                context.Depth.Pop();

                var depth = checked((uint)context.Depth.Count);
                var label = context.Labels[depth];

                if (!context.LoopLabels.Contains(label)) //Loop labels are marked where defined.
                    context.MarkLabel(label);
                else
                    context.LoopLabels.Remove(label);

                context.Labels.Remove(depth);
            }
        }
    }
}
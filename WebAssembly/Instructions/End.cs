using System.Reflection.Emit;
using WebAssembly.Runtime;
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
            Assert(context != null);
            Assert(context.Depth != null);

            var stack = context.Stack;
            Assert(stack != null);

            var blockType = context.Depth.Count == 0 ? BlockType.Empty : context.Depth.Pop();

            if (context.Depth.Count == 0)
            {
                if (context.Previous == OpCode.Return)
                    return; //WebAssembly requires functions to end on "end", but an immediately previous return is allowed.

                var returns = context.Signature.RawReturnTypes;
                var returnsLength = returns.Length;
                if (returnsLength != stack.Count)
                    throw new StackSizeIncorrectException(OpCode.End, returnsLength, stack.Count);

                Assert(returnsLength == 0 || returnsLength == 1); //WebAssembly doesn't currently offer multiple returns, which should be blocked earlier.

                if (returnsLength == 1)
                {
                    var type = stack.Pop();
                    if (type != returns[0])
                        throw new StackTypeInvalidException(OpCode.End, returns[0], type);
                }

                context.Emit(OpCodes.Ret);
            }
            else
            {
                if (blockType.TryToValueType(out var expectedType))
                {
                    var type = stack.Peek();
                    if (type != expectedType)
                        throw new StackTypeInvalidException(OpCode.End, expectedType, type);
                }

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
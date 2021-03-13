using System;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime.Compilation
{
    // Similar to System.Collections.Generic.Stack<T> but provides a very efficient ElementAt implementation.
    // Simplified in other ways since, being internal, it doesn't need to validate input or the provide the full Stack feature set.
    internal sealed class BlockStack
    {
        private BlockTypeInstruction?[] stack = Array.Empty<BlockTypeInstruction?>();
        public int Count { get; private set; }

        public void Clear()
        {
            if (Count == 0)
                return;

            Array.Clear(stack, 0, Count - 1);

            Count = 0;
        }

        public void Push(BlockTypeInstruction instruction)
        {
            if (stack.Length < Count + 1)
                Array.Resize(ref stack, Count + 128);

            stack[Count++] = instruction;
        }

        public BlockTypeInstruction Peek()
        {
            return stack[Count - 1]!;
        }

        public void PopNoReturn()
        {
            stack[--Count] = null;
        }

        public BlockTypeInstruction ElementAt(int index)
        {
            return stack[Count - index - 1]!;
        }
    }
}

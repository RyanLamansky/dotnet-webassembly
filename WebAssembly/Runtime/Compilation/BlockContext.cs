using System.Collections.Generic;

namespace WebAssembly.Runtime.Compilation
{
    /// <summary>
    /// Remembers stack state at the beginning of a block, and reachability of code.
    /// </summary>
    internal sealed class BlockContext
    {
        public readonly WebAssemblyValueType[] InitialStack;
        public bool IsUnreachable { get; private set; }

        public BlockContext()
        {
            IsUnreachable = false;
            InitialStack = new WebAssemblyValueType[0];
        }

        public BlockContext(Stack<WebAssemblyValueType> initialStack)
        {
            InitialStack = initialStack.ToArray();  //Copy stack
            IsUnreachable = false;
        }

        public void MarkUnreachable()
        {
            IsUnreachable = true;
        }

        public void MarkReachable()
        {
            IsUnreachable = false;
        }
    }
}

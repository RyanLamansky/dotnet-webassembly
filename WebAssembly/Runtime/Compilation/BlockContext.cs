using System.Collections.Generic;

namespace WebAssembly.Runtime.Compilation
{
    /// <summary>
    /// Remembers stack state at the beginning of a block, and reachability of code.
    /// </summary>
    internal sealed class BlockContext
    {
        public readonly int InitialStackSize;
        public bool IsUnreachable { get; private set; }

        public BlockContext()
        {
            IsUnreachable = false;
            InitialStackSize = 0;
        }

        public BlockContext(int initialStackSize)
        {
            InitialStackSize = initialStackSize;
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

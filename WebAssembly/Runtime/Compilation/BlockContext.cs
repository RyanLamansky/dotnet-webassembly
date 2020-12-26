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
        }

        public BlockContext(int initialStackSize)
        {
            InitialStackSize = initialStackSize;
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

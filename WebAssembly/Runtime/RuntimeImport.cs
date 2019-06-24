namespace WebAssembly.Runtime
{
    /// <summary>
    /// Functionality to integrate into a compiled WebAssembly instance.
    /// </summary>
    public abstract class RuntimeImport
    {
        /// <summary>
        /// The type of import.
        /// </summary>
        public abstract ExternalKind Kind { get; }

        private protected RuntimeImport()
        {
        }
    }
}
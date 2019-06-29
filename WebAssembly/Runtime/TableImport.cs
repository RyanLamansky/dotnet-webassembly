namespace WebAssembly.Runtime
{
    /// <summary>
    /// The base class for all table imports, which for the initial specification, is always <see cref="FunctionTable"/>.
    /// </summary>
    public abstract class TableImport : RuntimeImport
    {
        /// <summary>
        /// Always <see cref="ExternalKind.Table"/>.
        /// </summary>
        public sealed override ExternalKind Kind => ExternalKind.Table;

        /// <summary>
        /// The type of table being imported.
        /// </summary>
        public abstract ElementType Element { get; }

        private protected TableImport()
        {
        }
    }
}

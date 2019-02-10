namespace WebAssembly
{
    /// <summary>
    /// Many compiler tests can use this template to host the test.
    /// </summary>
    public abstract class CompilerTestBaseExportedImmutableGlobal<T>
        where T : struct
    {
        /// <summary>
        /// Creates a new <see cref="CompilerTestBaseExportedImmutableGlobal{T}"/> instance.
        /// </summary>
        protected CompilerTestBaseExportedImmutableGlobal()
        {
        }

        /// <summary>
        /// Returns a value.
        /// </summary>
        public abstract T Test { get; }
    }
}
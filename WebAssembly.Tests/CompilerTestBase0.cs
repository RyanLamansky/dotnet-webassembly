namespace WebAssembly
{
    /// <summary>
    /// Many compiler tests can use this template to host the test.
    /// </summary>
    public abstract class CompilerTestBase0<T>
        where T : struct
    {
        /// <summary>
        /// Creates a new <see cref="CompilerTestBase{T}"/> instance.
        /// </summary>
        protected CompilerTestBase0()
        {
        }

        /// <summary>
        /// Returns a value.
        /// </summary>
        /// <returns>A value to ensure proper control flow and execution.</returns>
        public abstract T Test();
    }
}
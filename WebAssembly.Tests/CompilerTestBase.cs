namespace WebAssembly
{
    /// <summary>
    /// Many compiler tests can use this template to host the test.
    /// </summary>
    public abstract class CompilerTestBase<T>
        where T : struct
    {
        /// <summary>
        /// Creates a new <see cref="CompilerTestBase{T}"/> instance.
        /// </summary>
        protected CompilerTestBase()
        {
        }

        /// <summary>
        /// Returns a value.
        /// </summary>
        /// <param name="parameter">Input to the test function.</param>
        /// <returns>A value to ensure proper control flow and execution.</returns>
        public abstract T Test(T parameter);

        /// <summary>
        /// Provides a <see cref="CompilerTestBase{T}"/> for the provided instructions.
        /// </summary>
        /// <param name="instructions">The instructions that form the body of the <see cref="Test(T)"/> function.</param>
        /// <returns>The <see cref="CompilerTestBase{T}"/> instance.</returns>
        public static CompilerTestBase<T> CreateInstance(params Instruction[] instructions)
        {
            var type = AssemblyBuilder.Map(typeof(T));

            return AssemblyBuilder.CreateInstance<CompilerTestBase<T>>(nameof(CompilerTestBase<T>.Test),
                type,
                new[]
                {
                    type,
                },
                instructions);
        }
    }
}
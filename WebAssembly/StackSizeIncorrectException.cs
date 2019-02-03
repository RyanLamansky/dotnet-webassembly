namespace WebAssembly
{
    /// <summary>
    /// Used by the compiler to indicate the stack was the wrong size to execute an operation.
    /// </summary>
    public class StackSizeIncorrectException : OpCodeCompilationException
    {
        /// <summary>
        /// Creates a new <see cref="StackSizeIncorrectException"/> with the provided parameters.
        /// </summary>
        /// <param name="opCode">The operation attempted.</param>
        /// <param name="expected">The expected stack height.</param>
        /// <param name="actual">The actual stack height at the time the operation was attempted.</param>
        public StackSizeIncorrectException(OpCode opCode, int expected, int actual)
            : base(opCode, $"requires at {expected} value{(expected != 1 ? "s" : "")} on the stack, found {actual}.")
        {
            this.Expected = expected;
            this.Actual = actual;
        }

        /// <summary>
        /// The expected stack height.
        /// </summary>
        public int Expected { get; }

        /// <summary>
        /// The actual stack height at the time the operation was attempted.
        /// </summary>
        public int Actual { get; }
    }
}
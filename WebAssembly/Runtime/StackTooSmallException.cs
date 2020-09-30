namespace WebAssembly.Runtime
{
    /// <summary>
    /// Used by the compiler to indicate the stack was too small to execute an operation.
    /// </summary>
    public class StackTooSmallException : OpCodeCompilationException
    {
        /// <summary>
        /// Creates a new <see cref="StackTooSmallException"/> with the provided parameters.
        /// </summary>
        /// <param name="opCode">The operation attempted.</param>
        /// <param name="minimum">The minimum acceptable stack height.</param>
        /// <param name="actual">The actual stack height at the time the operation was attempted.</param>
        public StackTooSmallException(OpCode opCode, int minimum, int actual)
            : base(opCode, $"requires at least {minimum} values on the stack, found {actual}.")
        {
            this.Minimum = minimum;
            this.Actual = actual;
        }

        /// <summary>
        /// Creates a new <see cref="StackTooSmallException"/> with the provided parameters.
        /// </summary>
        /// <param name="miscellaneousOpCode">The miscellaneous operation attempted.</param>
        /// <param name="minimum">The minimum acceptable stack height.</param>
        /// <param name="actual">The actual stack height at the time the operation was attempted.</param>
        public StackTooSmallException(MiscellaneousOpCode miscellaneousOpCode, int minimum, int actual)
            : base(miscellaneousOpCode, $"requires at least {minimum} values on the stack, found {actual}.")
        {
            this.Minimum = minimum;
            this.Actual = actual;
        }

        /// <summary>
        /// The minimum acceptable stack height.
        /// </summary>
        public int Minimum { get; }

        /// <summary>
        /// The actual stack height at the time the operation was attempted.
        /// </summary>
        public int Actual { get; }
    }
}
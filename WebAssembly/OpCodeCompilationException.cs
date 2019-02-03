namespace WebAssembly
{
    /// <summary>
    /// Used by the compiler to describe a problem encountered while compiling an <see cref="OpCode"/>.
    /// </summary>
    public class OpCodeCompilationException : CompilerException
    {
        /// <summary>
        /// Creates a new <see cref="OpCodeCompilationException"/> with the provided parameters.
        /// </summary>
        /// <param name="opCode">The operation attempted.</param>
        /// <param name="message">An explanation of the problem, concatenated with <paramref name="opCode"/> and passed to the base as the message.</param>
        public OpCodeCompilationException(OpCode opCode, string message)
            : base($"{opCode} {message}")
        {
            this.OpCode = opCode;
        }

        /// <summary>
        /// The operation attempted.
        /// </summary>
        public OpCode OpCode { get; }
    }
}
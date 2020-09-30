namespace WebAssembly.Runtime
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
        /// Creates a new <see cref="OpCodeCompilationException"/> with the provided parameters.
        /// </summary>
        /// <param name="miscellaneousOpCode">The miscellaneous operation attempted.</param>
        /// <param name="message">An explanation of the problem, concatenated with <paramref name="miscellaneousOpCode"/> and passed to the base as the message.</param>
        public OpCodeCompilationException(MiscellaneousOpCode miscellaneousOpCode, string message)
            : base($"{miscellaneousOpCode} {message}")
        {
            this.OpCode = OpCode.MiscellaneousOperationPrefix;
            this.MiscellaneousOpCode = miscellaneousOpCode;
        }

        /// <summary>
        /// The operation attempted.
        /// </summary>
        public OpCode OpCode { get; }

        /// <summary>
        /// The miscellaneous operation attempted, if <see cref="OpCode"/> is <see cref="OpCode.MiscellaneousOperationPrefix"/>.
        /// </summary>
        public MiscellaneousOpCode? MiscellaneousOpCode { get; }
    }
}
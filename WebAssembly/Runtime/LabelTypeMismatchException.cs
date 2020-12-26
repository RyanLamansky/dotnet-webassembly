namespace WebAssembly.Runtime
{
    /// <summary>
    /// Used by the compiler to indicate that the labels of a br_table instruction does not have the same type.
    /// </summary>
    public class LabelTypeMismatchException : OpCodeCompilationException
    {
        /// <summary>
        /// Creates a new <see cref="LabelTypeMismatchException"/> with the provided parameters.
        /// </summary>
        /// <param name="opCode">The operation attempted.</param>
        /// <param name="expected">The expected label type (type of the default label).</param>
        /// <param name="actual">The actual label type.</param>
        public LabelTypeMismatchException(OpCode opCode, BlockType expected, BlockType actual)
            : base(opCode, $"requires all labels to have type {expected}, but found {actual}.")
        {
            this.Expected = expected;
            this.Actual = actual;
        }

        /// <summary>
        /// The expected label type (type of the default label).
        /// </summary>
        public BlockType Expected { get; }

        /// <summary>
        /// The actual label type.
        /// </summary>
        public BlockType Actual { get; }
    }
}

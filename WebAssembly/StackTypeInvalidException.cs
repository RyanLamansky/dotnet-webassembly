
namespace WebAssembly
{
	/// <summary>
	/// Used by the compiler to indicate that the item at the top of the stack is invalid for an operation.
	/// </summary>
	public class StackTypeInvalidException : OpCodeCompilationException
	{
		/// <summary>
		/// Creates a new <see cref="StackTypeInvalidException"/> with the provided parameters.
		/// </summary>
		/// <param name="opCode">The operation attempted.</param>
		/// <param name="expected">The expected value type.</param>
		/// <param name="actual">The actual value type.</param>
		public StackTypeInvalidException(OpCode opCode, ValueType expected, ValueType actual)
			: base(opCode, $"requires the top stack item to be {expected}, found {actual}.")
		{
			this.Expected = expected;
			this.Actual = actual;
		}

		/// <summary>
		/// The expected value type.
		/// </summary>
		public ValueType Expected { get; }

		/// <summary>
		/// The actual value type.
		/// </summary>
		public ValueType Actual { get; }
	}
}
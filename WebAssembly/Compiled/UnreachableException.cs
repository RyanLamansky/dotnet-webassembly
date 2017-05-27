namespace WebAssembly.Compiled
{
	/// <summary>
	/// Indicates an <see cref="Instructions.Unreachable"/> instruction was reached.
	/// </summary>
	public class UnreachableException : RuntimeException
	{
		/// <summary>
		/// Creates a new <see cref="UnreachableException"/> with a default message: Unreachable instruction was encountered.
		/// </summary>
		public UnreachableException()
			: base("Unreachable instruction was encountered.")
		{
		}
	}
}
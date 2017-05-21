namespace WebAssembly
{
	/// <summary>
	/// Many compiler tests can use this template to host the test.
	/// </summary>
	public abstract class CompilerTestBase
	{
		/// <summary>
		/// Creates a new <see cref="CompilerTestBase"/> instance.
		/// </summary>
		protected CompilerTestBase()
		{
		}

		/// <summary>
		/// Returns a 32-bit integer.
		/// </summary>
		/// <param name="parameter">Input to the test function.</param>
		/// <returns>A value to ensure proper control flow and execution.</returns>
		public abstract int Test(int parameter);
	}
}
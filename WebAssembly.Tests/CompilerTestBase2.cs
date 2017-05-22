namespace WebAssembly
{
	/// <summary>
	/// Many compiler tests can use this template to host the test.
	/// </summary>
	public abstract class CompilerTestBase2
	{
		/// <summary>
		/// Creates a new <see cref="CompilerTestBase2"/> instance.
		/// </summary>
		protected CompilerTestBase2()
		{
		}

		/// <summary>
		/// Returns a 32-bit integer.
		/// </summary>
		/// <param name="parameter0">Input to the test function.</param>
		/// <param name="parameter1">Input to the test function.</param>
		/// <returns>A value to ensure proper control flow and execution.</returns>
		public abstract int Test(int parameter0, int parameter1);
	}
}
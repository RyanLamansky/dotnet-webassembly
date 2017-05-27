namespace WebAssembly
{
	/// <summary>
	/// This class simplifies building of memory-reading tests.
	/// </summary>
	/// <typeparam name="T">The type of value to read or write from memory.</typeparam>
	public abstract class MemoryReadTestBase<T>
	where T : struct
	{
		/// <summary>
		/// Creates a new <see cref="MemoryReadTestBase{T}"/> instance.
		/// </summary>
		protected MemoryReadTestBase()
		{
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		/// <param name="offset">Input to the test function.</param>
		/// <returns>A value to ensure proper control flow and execution.</returns>
		public abstract T Test(int offset);
	}
}
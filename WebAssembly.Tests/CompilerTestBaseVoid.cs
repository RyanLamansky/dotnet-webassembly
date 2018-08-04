namespace WebAssembly
{
	/// <summary>
	/// Many compiler tests can use this template to host the test.
	/// </summary>
	public abstract class CompilerTestBaseVoid<T>
		where T : struct
	{
		/// <summary>
		/// Creates a new <see cref="CompilerTestBaseVoid{T}"/> instance.
		/// </summary>
		protected CompilerTestBaseVoid()
		{
		}

		/// <summary>
		/// A test function.
		/// </summary>
		/// <param name="parameter0">Input to the test function.</param>
		public abstract void Test(T parameter0);

		/// <summary>
		/// Provides a <see cref="CompilerTestBase2{T}"/> for the provided instructions.
		/// </summary>
		/// <param name="instructions">The instructions that form the body of the <see cref="Test(T)"/> function.</param>
		/// <returns>The <see cref="CompilerTestBase2{T}"/> instance.</returns>
		public static CompilerTestBase2<T> CreateInstance(params Instruction[] instructions)
		{
			var type = AssemblyBuilder.Map(typeof(T));

			return AssemblyBuilder.CreateInstance<CompilerTestBase2<T>>(nameof(CompilerTestBase2<T>.Test),
				type,
				new[]
				{
					type,
					type,
				},
				instructions);
		}
	}
}
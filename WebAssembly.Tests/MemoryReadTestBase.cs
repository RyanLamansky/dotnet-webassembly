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

		/// <summary>
		/// Provides a <see cref="MemoryReadTestBase{T}"/> for the provided instructions.
		/// </summary>
		/// <param name="instructions">The instructions that form the body of the <see cref="Test"/> function.</param>
		/// <returns>The created instance.</returns>
		public static Instance<MemoryReadTestBase<T>, object> CreateInstance(params Instruction[] instructions)
		{
			var module = new Module();
			module.Memories.Add(new Memory(1, 1));
			module.Types.Add(new Type
			{
				Returns = new[] { AssemblyBuilder.Map(typeof(T)) },
				Parameters = new[] { ValueType.Int32 },
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(MemoryReadTestBase<T>.Test),
			});
			module.Codes.Add(new FunctionBody
			{
				Code = instructions,
			});

			Instance<MemoryReadTestBase<T>, object> compiled;
			using (var memory = new System.IO.MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				var maker = Compile.FromBinary<MemoryReadTestBase<T>, object>(memory);
				compiled = maker();
			}

			return compiled;
		}
	}
}
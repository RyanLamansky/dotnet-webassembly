namespace WebAssembly
{
	/// <summary>
	/// This class simplifies building of memory-writing tests.
	/// </summary>
	/// <typeparam name="T">The type of value to read or write from memory.</typeparam>
	public abstract class MemoryWriteTestBase<T>
	where T : struct
	{
		/// <summary>
		/// Creates a new <see cref="MemoryWriteTestBase{T}"/> instance.
		/// </summary>
		protected MemoryWriteTestBase()
		{
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		/// <param name="offset">Input to the test function.</param>
		/// <param name="value">The value to write.</param>
		public abstract void Test(int offset, T value);

		/// <summary>
		/// The memory associated with the instance.
		/// </summary>
		public abstract Runtime.UnmanagedMemory Memory { get; }

		/// <summary>
		/// Provides a <see cref="MemoryWriteTestBase{T}"/> for the provided instructions.
		/// </summary>
		/// <param name="instructions">The instructions that form the body of the <see cref="Test"/> function.</param>
		/// <returns>The created instance.</returns>
		public static Instance<MemoryWriteTestBase<T>> CreateInstance(params Instruction[] instructions)
		{
			var module = new Module();
			module.Memories.Add(new Memory(1, 1));
			module.Types.Add(new Type
			{
				Parameters = new[]
				{
					ValueType.Int32,
					AssemblyBuilder.Map(typeof(T)),
				},
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(MemoryWriteTestBase<T>.Test),
			});
			module.Exports.Add(new Export
			{
				Name = "Memory",
				Kind = ExternalKind.Memory,
			});
			module.Codes.Add(new FunctionBody
			{
				Code = instructions,
			});

			return module.ToInstance<MemoryWriteTestBase<T>>(); ;
		}
	}
}
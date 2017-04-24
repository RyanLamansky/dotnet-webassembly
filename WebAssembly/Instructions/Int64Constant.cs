namespace WebAssembly.Instructions
{
	/// <summary>
	/// Produce the value of an i64 immediate.
	/// </summary>
	public class Int64Constant : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Constant"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Constant;

		/// <summary>
		/// Gets or sets the value of the constant.
		/// </summary>
		public long Value { get; set; }

		/// <summary>
		/// Creates a new <see cref="Int64Constant"/> instance.
		/// </summary>
		public Int64Constant()
		{
		}

		/// <summary>
		/// Creates a new <see cref="Int64Constant"/> instance with the provided value.
		/// </summary>
		/// <param name="value">The value of the constant.  This is passed to the <see cref="Value"/> property.</param>
		public Int64Constant(long value) => Value = value;

		/// <summary>
		/// Creates a new <see cref="Int64Constant"/> instance from binary data.
		/// </summary>
		/// <param name="reader">The source of binary data.</param>
		internal Int64Constant(Reader reader)
		{
			Value = reader.ReadVarInt64();
		}
	}
}
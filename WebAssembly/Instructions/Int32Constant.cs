namespace WebAssembly.Instructions
{
	/// <summary>
	/// Produce the value of an i32 immediate.
	/// </summary>
	public class Int32Constant : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Constant"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Constant;

		/// <summary>
		/// Gets or sets the value of the constant.
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// Creates a new <see cref="Int32Constant"/> instance.
		/// </summary>
		public Int32Constant()
		{
		}

		/// <summary>
		/// Creates a new <see cref="Int32Constant"/> instance with the provided value.
		/// </summary>
		/// <param name="value">The value of the constant.  This is passed to the <see cref="Value"/> property.</param>
		public Int32Constant(int value) => Value = value;

		/// <summary>
		/// Creates a new <see cref="Int32Constant"/> instance from binary data.
		/// </summary>
		/// <param name="reader">The source of binary data.</param>
		internal Int32Constant(Reader reader)
		{
			Value = reader.ReadVarInt32();
		}

		internal override void WriteTo(Writer writer)
		{
			writer.Write((byte)OpCode.Int32Constant);
			writer.WriteVar(this.Value);
		}
	}
}
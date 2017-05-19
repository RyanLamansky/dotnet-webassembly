namespace WebAssembly.Instructions
{
	/// <summary>
	/// Produce the value of an f64 immediate.
	/// </summary>
	public class Float64Constant : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Constant"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Constant;

		/// <summary>
		/// Gets or sets the value of the constant.
		/// </summary>
		public double Value { get; set; }

		/// <summary>
		/// Creates a new <see cref="Float64Constant"/> instance.
		/// </summary>
		public Float64Constant()
		{
		}

		/// <summary>
		/// Creates a new <see cref="Float64Constant"/> instance with the provided value.
		/// </summary>
		/// <param name="value">The value of the constant.  This is passed to the <see cref="Value"/> property.</param>
		public Float64Constant(double value) => Value = value;

		/// <summary>
		/// Creates a new <see cref="Float64Constant"/> instance from binary data.
		/// </summary>
		/// <param name="reader">The source of binary data.</param>
		internal Float64Constant(Reader reader)
		{
			Value = reader.ReadFloat64();
		}

		internal override void WriteTo(Writer writer)
		{
			writer.Write((byte)OpCode.Float64Constant);
			writer.Write(this.Value);
		}
	}
}
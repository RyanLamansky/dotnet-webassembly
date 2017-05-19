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

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) =>
			other is Int32Constant instruction
			&& instruction.Value == this.Value
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Value);
	}
}
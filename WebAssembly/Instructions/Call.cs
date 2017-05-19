using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Call function directly.
	/// </summary>
	public class Call : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Call"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Call;

		/// <summary>
		/// The location within the function index to call.
		/// </summary>
		public uint Index { get; set; }

		/// <summary>
		/// Creates a new  <see cref="Call"/> instance.
		/// </summary>
		public Call()
		{
		}

		internal Call(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Index = reader.ReadVarUInt32();
		}

		internal sealed override void WriteTo(Writer writer)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			writer.Write((byte)OpCode.Call);
			writer.WriteVar(this.Index);
		}

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) =>
			other is Call instruction
			&& instruction.Index == this.Index
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);
	}
}
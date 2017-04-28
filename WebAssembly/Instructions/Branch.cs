using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Branch to a given label in an enclosing construct.
	/// </summary>
	public class Branch : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Branch"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Branch;

		/// <summary>
		/// The number of ancestor blocks to climb; 0 is the immediate parent.
		/// </summary>
		public uint Index { get; set; }

		/// <summary>
		/// Creates a new  <see cref="Branch"/> instance.
		/// </summary>
		public Branch()
		{
		}

		internal Branch(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Index = reader.ReadVarUInt32();
		}
	}
}
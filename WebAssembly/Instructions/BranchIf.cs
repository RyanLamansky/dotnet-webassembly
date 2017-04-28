using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Conditionally branch to a given label in an enclosing construct.
	/// </summary>
	public class BranchIf : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.BranchIf"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.BranchIf;

		/// <summary>
		/// The number of ancestor blocks to climb; 0 is the immediate parent.
		/// </summary>
		public uint Index { get; set; }

		/// <summary>
		/// Creates a new  <see cref="BranchIf"/> instance.
		/// </summary>
		public BranchIf()
		{
		}

		internal BranchIf(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Index = reader.ReadVarUInt32();
		}
	}
}
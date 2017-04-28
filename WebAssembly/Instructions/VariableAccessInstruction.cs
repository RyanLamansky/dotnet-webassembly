using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// An instruction that access a variable.
	/// </summary>
	public abstract class VariableAccessInstruction : Instruction
	{
		/// <summary>
		/// The index of the variable to access.
		/// </summary>
		public uint Index { get; set; }

		internal VariableAccessInstruction(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Index = reader.ReadVarUInt32();
		}

		/// <summary>
		/// Creates a new <see cref="VariableAccessInstruction"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		internal VariableAccessInstruction(uint index)
		{
			this.Index = index;
		}

		/// <summary>
		/// Creates a new <see cref="VariableAccessInstruction"/> instance.
		/// </summary>
		internal VariableAccessInstruction()
		{
		}
	}
}
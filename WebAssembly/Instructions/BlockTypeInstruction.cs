using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Supports instructions that use the "block_type" data fields from the binary encoding specification.
	/// </summary>
	public abstract class BlockTypeInstruction : Instruction
    {
		/// <summary>
		/// The type of value on the stack when the block exits, or <see cref="BlockType.Empty"/> if none.
		/// </summary>
		public BlockType Type { get; set; }

		/// <summary>
		/// Creates a new <see cref="BlockType"/> instance.
		/// </summary>
		internal BlockTypeInstruction()
		{
			this.Type = BlockType.Empty;
		}

		/// <summary>
		/// Creates a new <see cref="BlockType"/> instance from the provided data stream.
		/// </summary>
		/// <param name="reader">Reads the bytes of a web assembly binary file.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
		internal BlockTypeInstruction(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Type = (BlockType)reader.ReadVarInt7();
		}
	}
}
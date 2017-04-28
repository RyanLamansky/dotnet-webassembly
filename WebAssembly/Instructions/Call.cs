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
	}
}
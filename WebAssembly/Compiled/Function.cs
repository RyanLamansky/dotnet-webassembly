using System.Linq;

namespace WebAssembly.Compiled
{
	internal sealed class Function
	{
		public struct Local
		{
			public Local(Reader reader)
			{
				this.Count = reader.ReadVarUInt32();
				this.Type = (ValueType)reader.ReadVarInt7();
			}

			public readonly uint Count;
			public readonly ValueType Type;
		}

		public readonly Signature Signature;
		public readonly Local[] Locals;
		public readonly Instruction[] Instructions;

		public Function(Reader reader, Signature signature, long byteLength)
		{
			this.Signature = signature;

			var startingOffset = reader.Offset;
			var locals = this.Locals = new Local[reader.ReadVarUInt32()];
			for (var i = 0; i < locals.Length; i++)
				locals[i] = new Local(reader);

			this.Instructions = Instruction.Parse(reader).ToArray();

			if (reader.Offset - startingOffset != byteLength)
				throw new ModuleLoadException($"Instruction sequence reader ended after readering {reader.Offset - startingOffset} characters, expected {byteLength}.", reader.Offset);
		}
	}
}
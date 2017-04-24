using System;
using System.Collections.Generic;

namespace WebAssembly
{
	using Instructions;

	/// <summary>
	/// A combination of <see cref="OpCode"/> and its associated parameters.
	/// </summary>
	public abstract class Instruction
	{
		/// <summary>
		/// Creates a new <see cref="Instruction"/> instance.
		/// </summary>
		internal Instruction()
		{
		}

		/// <summary>
		/// Gets the <see cref="OpCode"/> associated with this instruction.
		/// </summary>
		public abstract OpCode OpCode { get; }

		/// <summary>
		/// Parses an instruction stream restricted to the opcodes available for an initializer expression.
		/// </summary>
		/// <param name="reader">The source of binary data.</param>
		/// <returns>Parsed instructions.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
		internal static IEnumerable<Instruction> ParseInitializerExpression(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			//As of the initial version, the set of operations valid for initializer experssions is extremely limited.
			while (true)
			{
				var opCode = (OpCode)reader.ReadByte();
				switch (opCode)
				{
					default: throw new ModuleLoadException($"Don't know how to parse opcode \"{opCode}\".", reader.Offset);
					case OpCode.Int32Constant: yield return new Int32Constant(reader); break;
					case OpCode.Int64Constant: yield return new Int64Constant(reader); break;
					case OpCode.Float32Constant: yield return new Float32Constant(reader); break;
					case OpCode.Float64Constant: yield return new Float64Constant(reader); break;
					case OpCode.End: yield return new End(); yield break;
				}
			}
		}
	}
}
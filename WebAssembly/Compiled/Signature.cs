using System;

namespace WebAssembly.Compiled
{
	internal sealed class Signature
	{
		public readonly System.Type[] param_types;
		public readonly System.Type[] return_types;

		public Signature(Reader reader)
		{
			reader.ReadVarInt7(); //Function Type

			var parameters = this.param_types = new System.Type[reader.ReadVarUInt32()];

			System.Type Map(ValueType type)
			{
				switch (type)
				{
					case ValueType.Int32: return typeof(int);
					case ValueType.Int64: return typeof(long);
					case ValueType.Float32: return typeof(float);
					case ValueType.Float64: return typeof(double);
					default: throw new ArgumentOutOfRangeException(nameof(type), $"Type {type} not recognized.");
				}
			}

			for (var i = 0; i < parameters.Length; i++)
				parameters[i] = Map((ValueType)reader.ReadVarInt7());

			var returns = this.return_types = new System.Type[reader.ReadVarUInt1()];

			if (returns.Length > 1)
				throw new ModuleLoadException("Multiple returns are not supported.", reader.Offset);

			for (var i = 0; i < returns.Length; i++)
				returns[i] = Map((ValueType)reader.ReadVarInt7());
		}
	}
}
using System;

namespace WebAssembly.Compiled
{
	internal sealed class Signature
	{
		public readonly System.Type[] ParameterTypes;
		public readonly ValueType[] RawParameterTypes;
		public readonly System.Type[] ReturnTypes;
		public readonly ValueType[] RawReturnTypes;

		public Signature(Reader reader)
		{
			reader.ReadVarInt7(); //Function Type

			var parameters = this.ParameterTypes = new System.Type[reader.ReadVarUInt32()];
			var rawParameters = this.RawParameterTypes = new ValueType[parameters.Length];

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
				parameters[i] = Map(rawParameters[i] = (ValueType)reader.ReadVarInt7());

			var returns = this.ReturnTypes = new System.Type[reader.ReadVarUInt1()];
			var rawReturns = this.RawReturnTypes = new ValueType[returns.Length];

			if (returns.Length > 1)
				throw new ModuleLoadException("Multiple returns are not supported.", reader.Offset);

			for (var i = 0; i < returns.Length; i++)
				returns[i] = Map(rawReturns[i] = (ValueType)reader.ReadVarInt7());
		}
	}
}
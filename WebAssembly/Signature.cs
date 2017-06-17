namespace WebAssembly
{
	internal sealed class Signature
	{
		public readonly uint TypeIndex;
		public readonly System.Type[] ParameterTypes;
		public readonly ValueType[] RawParameterTypes;
		public readonly System.Type[] ReturnTypes;
		public readonly ValueType[] RawReturnTypes;

		private static readonly RegeneratingWeakReference<Signature> empty = new RegeneratingWeakReference<Signature>(() => new Signature());

		public static Signature Empty => empty;

		private Signature()
		{
			this.TypeIndex = uint.MaxValue;
			this.ReturnTypes = this.ParameterTypes = new System.Type[0];
			this.RawReturnTypes = this.RawParameterTypes = new ValueType[0];
		}

		public Signature(ValueType returnType)
		{
			this.TypeIndex = uint.MaxValue;
			this.ParameterTypes = new System.Type[0];
			this.RawParameterTypes = new ValueType[0];
			this.ReturnTypes = new[] { returnType.ToSystemType() };
			this.RawReturnTypes = new[] { returnType };
		}

		public Signature(Reader reader, uint typeIndex)
		{
			this.TypeIndex = typeIndex;

			reader.ReadVarInt7(); //Function Type

			var parameters = this.ParameterTypes = new System.Type[reader.ReadVarUInt32()];
			var rawParameters = this.RawParameterTypes = new ValueType[parameters.Length];

			for (var i = 0; i < parameters.Length; i++)
				parameters[i] = (rawParameters[i] = (ValueType)reader.ReadVarInt7()).ToSystemType();

			var returns = this.ReturnTypes = new System.Type[reader.ReadVarUInt1()];
			var rawReturns = this.RawReturnTypes = new ValueType[returns.Length];

			if (returns.Length > 1)
				throw new ModuleLoadException("Multiple returns are not supported.", reader.Offset);

			for (var i = 0; i < returns.Length; i++)
				returns[i] = (rawReturns[i] = (ValueType)reader.ReadVarInt7()).ToSystemType();
		}
	}
}
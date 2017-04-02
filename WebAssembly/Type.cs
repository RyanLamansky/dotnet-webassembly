using System.Collections.Generic;

namespace WebAssembly
{
	/// <summary>
	/// Describes the signature of a function.
	/// </summary>
	public class Type
	{
		/// <summary>
		/// The type of function.  The only accepted value in the initial binary format is <see cref="FunctionType.Function"/>, which is the default.
		/// </summary>
		public FunctionType Form { get; set; }

		private IList<ValueType> parameters;

		/// <summary>
		/// Parameters to the function.
		/// </summary>
		public IList<ValueType> Parameters
		{
			get => this.parameters ?? (this.parameters = new List<ValueType>());
			set => this.parameters = value ?? new List<ValueType>();
		}

		private IList<ValueType> returns;

		/// <summary>
		/// Return types to the function.  For the initial binary format, a maximum of 1 is allowed.
		/// </summary>
		public IList<ValueType> Returns
		{
			get => this.returns ?? (this.returns = new List<ValueType>());
			set => this.returns = value ?? new List<ValueType>();
		}

		/// <summary>
		/// Creates a new <see cref="Type"/> instance.
		/// </summary>
		public Type()
		{
			this.Form = FunctionType.Function;
		}

		internal Type(Reader reader)
		{
			this.Form = (FunctionType)reader.ReadVarInt7();
			int count;

			count = checked((int)reader.ReadVarUInt32());
			var parameters = this.parameters = new List<ValueType>(count);

			for (var i = 0; i < count; i++)
				parameters.Add((ValueType)reader.ReadVarInt7());

			count = checked((int)reader.ReadVarUInt32());
			var returns = this.returns = new List<ValueType>(count);
			for (var i = 0; i < count; i++)
				returns.Add((ValueType)reader.ReadVarInt7());
		}
	}
}
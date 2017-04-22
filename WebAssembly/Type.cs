using System;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.Debug;

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
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<ValueType> Parameters
		{
			get => this.parameters ?? (this.parameters = new List<ValueType>());
			set => this.parameters = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<ValueType> returns;

		/// <summary>
		/// Return types to the function.  For the initial binary format, a maximum of 1 is allowed.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<ValueType> Returns
		{
			get => this.returns ?? (this.returns = new List<ValueType>());
			set => this.returns = value ?? throw new ArgumentNullException(nameof(value));
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

		/// <summary>
		/// Expresses the value of this instance as a string.
		/// </summary>
		/// <returns>A string representation of this instance.</returns>
		public override string ToString()
		{
			var parameters = this.Parameters;
			var returns = this.Returns;

			Assert(parameters != null);
			Assert(returns != null);

			var builder = new StringBuilder("Form: ")
				.Append(this.Form.ToString())
				.Append("; Parameters: ")
				;

			for (var i = 0; i < parameters.Count; i++)
			{
				if (i != 0)
					builder.Append(", ");
				builder.Append(parameters[i]);
			}

			if (returns.Count == 0)
			{
				builder.Append("; (No Returns)");
			}
			else
			{
				builder.Append("; Returns: ");

				for (var i = 0; i < returns.Count; i++)
				{
					if (i != 0)
						builder.Append(", ");
					builder.Append(returns[i]);
				}
			}

			return builder.ToString();
		}
	}
}
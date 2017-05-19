using System;

namespace WebAssembly
{
	/// <summary>
	/// Describes external features made available to the web assembly.
	/// </summary>
	public abstract class Import
	{
		private string module;

		/// <summary>
		/// The module portion of the name.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public string Module
		{
			get => module ?? "";
			set => module = value ?? throw new ArgumentNullException(nameof(value));
		}

		private string field;

		/// <summary>
		/// The field portion of the name.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public string Field
		{
			get => field ?? "";
			set => field = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// The type of import.
		/// </summary>
		public abstract ExternalKind Kind { get; }

		/// <summary>
		/// Creates a new <see cref="Import"/> instance.
		/// </summary>
		internal Import()
		{
		}

		/// <summary>
		/// Expresses the value of this instance as a string.
		/// </summary>
		/// <returns>A string representation of this instance.</returns>
		public override string ToString() => $"{Module}::{Field}";

		internal virtual void WriteTo(Writer writer)
		{
			writer.Write(this.Module);
			writer.Write(this.Field);
		}

		/// <summary>
		/// Creates an <see cref="Import"/> instance from the provided <see cref="Reader"/>.
		/// </summary>
		/// <param name="reader">Provides raw data.</param>
		/// <returns>The parsed <see cref="Import"/>.</returns>
		/// <exception cref="ModuleLoadException">Imported external kind is not recognized or not supported.</exception>
		internal static Import ParseFrom(Reader reader)
		{
			var module = reader.ReadString(reader.ReadVarUInt32());
			var field = reader.ReadString(reader.ReadVarUInt32());
			var kind = (ExternalKind)reader.ReadByte();

			switch (kind)
			{
				case ExternalKind.Function:
					return new Function
					{
						Module = module,
						Field = field,
						TypeIndex = reader.ReadVarUInt32(),
					};

				case ExternalKind.Table:
				case ExternalKind.Memory:
				case ExternalKind.Global:
					throw new ModuleLoadException($"Imported external kind of {kind} is not currently supported.", reader.Offset);

				default:
					throw new ModuleLoadException($"Imported external kind of {kind} is not recognized.", reader.Offset);
			}
		}

		/// <summary>
		/// Describes an imported function.
		/// </summary>
		public class Function : Import
		{
			/// <summary>
			/// Always <see cref="ExternalKind.Function"/>.
			/// </summary>
			public sealed override ExternalKind Kind => ExternalKind.Function;

			/// <summary>
			/// The index within the module's types that describes the function signature.
			/// </summary>
			public uint TypeIndex { get; set; }

			/// <summary>
			/// Creates a new <see cref="Function"/> instance.
			/// </summary>
			public Function()
			{
			}

			/// <summary>
			/// Expresses the value of this instance as a string.
			/// </summary>
			/// <returns>A string representation of this instance.</returns>
			public override string ToString() => $"{base.ToString()} (Function {TypeIndex}";

			internal override void WriteTo(Writer writer)
			{
				base.WriteTo(writer);
				writer.Write((byte)ExternalKind.Function);
				writer.WriteVar(this.TypeIndex);
			}
		}
	}
}
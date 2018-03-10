using System;
using System.Diagnostics;

namespace WebAssembly
{
	/// <summary>
	/// Describes external features made available to the web assembly.
	/// </summary>
	public abstract class Import
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
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

		[DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
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
		private protected Import()
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
			var initialOffset = reader.Offset;
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
					return new Table
					{
						Module = module,
						Field = field,
						Type = new WebAssembly.Table(reader),
					};

				case ExternalKind.Memory:
					return new Memory
					{
						Module = module,
						Field = field,
						Type = new WebAssembly.Memory(reader),
					};

				case ExternalKind.Global:
					return new Global
					{
						Module = module,
						Field = field,
						Type = new WebAssembly.Global(reader),
					};

				default:
					throw new ModuleLoadException($"Imported external kind of {kind} is not recognized.", initialOffset);
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

			internal sealed override void WriteTo(Writer writer)
			{
				base.WriteTo(writer);
				writer.Write((byte)ExternalKind.Function);
				writer.WriteVar(this.TypeIndex);
			}
		}

		/// <summary>
		/// Describes an imported table.
		/// </summary>
		public class Table : Import
		{
			/// <summary>
			/// Always <see cref="ExternalKind.Table"/>.
			/// </summary>
			public sealed override ExternalKind Kind => ExternalKind.Table;

			/// <summary>
			/// Type of the imported table.
			/// </summary>
			public WebAssembly.Table Type { get; set; }

			/// <summary>
			/// Creates a new <see cref="Table"/> instance.
			/// </summary>
			public Table()
			{
			}

			/// <summary>
			/// Expresses the value of this instance as a string.
			/// </summary>
			/// <returns>A string representation of this instance.</returns>
			public override string ToString() => $"{base.ToString()} (Function {Type}";

			internal sealed override void WriteTo(Writer writer)
			{
				base.WriteTo(writer);
				writer.Write((byte)ExternalKind.Table);
				Type.WriteTo(writer);
			}
		}

		/// <summary>
		/// Describes an imported memory.
		/// </summary>
		public class Memory : Import
		{
			/// <summary>
			/// Always <see cref="ExternalKind.Memory"/>.
			/// </summary>
			public sealed override ExternalKind Kind => ExternalKind.Memory;

			/// <summary>
			/// Type of the imported memory.
			/// </summary>
			public WebAssembly.Memory Type { get; set; }

			/// <summary>
			/// Creates a new <see cref="Memory"/> instance.
			/// </summary>
			public Memory()
			{
			}

			/// <summary>
			/// Expresses the value of this instance as a string.
			/// </summary>
			/// <returns>A string representation of this instance.</returns>
			public override string ToString() => $"{base.ToString()} (Function {Type}";

			internal sealed override void WriteTo(Writer writer)
			{
				base.WriteTo(writer);
				writer.Write((byte)ExternalKind.Memory);
				Type.WriteTo(writer);
			}
		}

		/// <summary>
		/// Describes an imported global.
		/// </summary>
		public class Global : Import
		{
			/// <summary>
			/// Always <see cref="ExternalKind.Global"/>.
			/// </summary>
			public sealed override ExternalKind Kind => ExternalKind.Global;

			/// <summary>
			/// Type of the imported global.
			/// </summary>
			public WebAssembly.Global Type { get; set; }

			/// <summary>
			/// Creates a new <see cref="Global"/> instance.
			/// </summary>
			public Global()
			{
			}

			/// <summary>
			/// Expresses the value of this instance as a string.
			/// </summary>
			/// <returns>A string representation of this instance.</returns>
			public override string ToString() => $"{base.ToString()} (Function {Type}";

			internal sealed override void WriteTo(Writer writer)
			{
				base.WriteTo(writer);
				writer.Write((byte)ExternalKind.Global);
				Type.WriteTo(writer);
			}
		}
	}
}
using System;
using System.Collections.Generic;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// A jump table which jumps to a label in an enclosing construct.
	/// </summary>
	public class BranchTable : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.BranchTable"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.BranchTable;

		private IList<uint> labels;

		/// <summary>
		/// A zero-based array of labels.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<uint> Labels
		{
			get => this.labels ?? (this.labels = new List<uint>());
			set => this.labels = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// The default label if the jump is out of bounds.
		/// </summary>
		public uint DefaultLabel { get; set; }

		/// <summary>
		/// Creates a new  <see cref="BranchTable"/> instance.
		/// </summary>
		public BranchTable()
		{
		}

		internal BranchTable(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			var count = reader.ReadVarUInt32();
			var labels = new List<uint>();
			Labels = labels;

			for (var i = 0; i < count; i++)
				labels.Add(reader.ReadVarUInt32());

			this.DefaultLabel = reader.ReadVarUInt32();
		}

		internal sealed override void WriteTo(Writer writer)
		{
			writer.Write((byte)OpCode.BranchTable);

			var labels = this.Labels;
			writer.WriteVar((uint)labels.Count);

			foreach (var label in labels)
				writer.WriteVar(label);
			writer.WriteVar(this.DefaultLabel);
		}
	}
}
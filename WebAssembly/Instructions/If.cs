using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// The beginning of an if construct with an implicit "then" block.
	/// </summary>
	public class If : BlockTypeInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.If"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.If;

		/// <summary>
		/// Creates a new  <see cref="If"/> instance.
		/// </summary>
		public If()
		{
		}

		/// <summary>
		/// Creates a new <see cref="If"/> of the provided type.
		/// </summary>
		/// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
		public If(BlockType type)
			: base(type)
		{
		}

		internal If(Reader reader)
			: base(reader)
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var label = context.DefineLabel();
			context.Labels.Add(context.Depth++, label);
			context.Emit(OpCodes.Brfalse, label);
		}
	}
}
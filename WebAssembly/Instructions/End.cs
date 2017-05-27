using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// An instruction that marks the end of a block, loop, if, or function.
	/// </summary>
	public class End : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.End"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.End;

		/// <summary>
		/// Creates a new <see cref="End"/> instance.
		/// </summary>
		public End()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			Assert(context.Depth > 0);
			if (--context.Depth == 0)
				context.Emit(OpCodes.Ret);
			else
			{
				var label = context.Labels[context.Depth];

				if (!context.LoopLabels.Contains(label)) //Loop labels are marked where defined.
					context.MarkLabel(label);
				else
					context.LoopLabels.Remove(label);

				context.Labels.Remove(context.Depth);
			}
		}
	}
}
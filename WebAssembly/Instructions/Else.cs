using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Marks the else block of an <see cref="If"/>.
	/// </summary>
	public class Else : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Else"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Else;

		/// <summary>
		/// Creates a new  <see cref="Else"/> instance.
		/// </summary>
		public Else()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			var afterElse = context.DefineLabel();
			context.Emit(OpCodes.Br, afterElse);

			var target = checked((uint)context.Depth.Count) - 1;
			context.MarkLabel(context.Labels[target]);
			context.Labels[target] = afterElse;
		}
	}
}
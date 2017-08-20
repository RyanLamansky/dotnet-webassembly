using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// No operation, no effect.
	/// </summary>
	public class NoOperation : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.NoOperation"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.NoOperation;

		/// <summary>
		/// Creates a new  <see cref="NoOperation"/> instance.
		/// </summary>
		public NoOperation()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			context.Emit(OpCodes.Nop);
		}
	}
}
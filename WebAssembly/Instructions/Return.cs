using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Return zero or more values from this function.
	/// </summary>
	public class Return : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Return"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Return;

		/// <summary>
		/// Creates a new  <see cref="Return"/> instance.
		/// </summary>
		public Return()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			context.Emit(OpCodes.Ret);
		}
	}
}
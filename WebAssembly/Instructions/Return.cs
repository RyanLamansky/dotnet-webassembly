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
			var returns = context.Signature.RawReturnTypes;
			if (returns.Length != 0)
			{
				var stack = context.Stack;
				if (stack.Count == 0)
					throw new StackTooSmallException(OpCode.Return, 1, 0);

				var type = stack.Pop();
				if (type != returns[0])
					throw new StackTypeInvalidException(OpCode.Return, returns[0], type);
			}

			context.Emit(OpCodes.Ret);
		}
	}
}
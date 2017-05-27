using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic addition.
	/// </summary>
	public class Int32Add : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Add;

		/// <summary>
		/// Creates a new  <see cref="Int32Add"/> instance.
		/// </summary>
		public Int32Add()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 2)
				throw new StackTooSmallException(OpCode.Int32Add, 2, stack.Count);
		
			var typeB = stack.Pop();
			var typeA = stack.Peek(); //Assuming validation passes, the remaining type will be this.

			if (typeA != ValueType.Int32)
				throw new StackTypeInvalidException(OpCode.Int32Add, ValueType.Int32, typeA);

			if (typeA != typeB)
				throw new StackParameterMismatchException(OpCode.Int32Add, typeA, typeB);

			context.Emit(OpCodes.Add);
		}
	}
}
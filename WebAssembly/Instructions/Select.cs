using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// A ternary operator with two operands, which have the same type as each other, plus a boolean (i32) condition. Returns the first operand if the condition operand is non-zero, or the second otherwise.
	/// </summary>
	public class Select : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Select"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Select;

		/// <summary>
		/// Creates a new  <see cref="Select"/> instance.
		/// </summary>
		public Select()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 3)
				throw new StackTooSmallException(OpCode.Select, 3, stack.Count);

			var type = stack.Pop();
			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(OpCode.Select, ValueType.Int32, type);

			var typeB = stack.Pop();
			var typeA = stack.Peek(); //Assuming validation passes, the remaining type will be this.

			if (typeA != typeB)
				throw new StackParameterMismatchException(OpCode.Select, typeA, typeB);

			HelperMethod helper;
			switch (typeA)
			{
				default: //This shouldn't be possible due to previous validations.
				case ValueType.Int32: helper = HelperMethod.SelectInt32; break;
				case ValueType.Int64: helper = HelperMethod.SelectInt64; break;
				case ValueType.Float32: helper = HelperMethod.SelectFloat32; break;
				case ValueType.Float64: helper = HelperMethod.SelectFloat64; break;
			}
			context.Emit(OpCodes.Call, context[helper]);
		}
	}
}
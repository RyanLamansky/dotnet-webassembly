namespace WebAssembly.Instructions
{
	/// <summary>
	/// Identifies an instruction that uses a single CIL <see cref="System.Reflection.Emit.OpCode"/> to remove two values of the same type from the stackm, returning a single <see cref="ValueType.Int32"/>.
	/// </summary>
	public abstract class ValueTwoToInt32Instruction : SimpleInstruction
	{
		internal ValueTwoToInt32Instruction()
		{
		}

		internal abstract ValueType ValueType { get; }

		internal abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 2)
				throw new StackTooSmallException(this.OpCode, 2, stack.Count);

			var typeB = stack.Pop();
			var typeA = stack.Pop();

			if (typeA != this.ValueType)
				throw new StackTypeInvalidException(this.OpCode, this.ValueType, typeA);

			if (typeA != typeB)
				throw new StackParameterMismatchException(this.OpCode, typeA, typeB);

			stack.Push(ValueType.Int32);

			context.Emit(this.EmittedOpCode);
		}
	}
}
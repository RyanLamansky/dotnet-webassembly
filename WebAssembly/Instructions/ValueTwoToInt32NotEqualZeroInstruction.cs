using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Identifies an instruction that uses a single CIL <see cref="System.Reflection.Emit.OpCode"/> to remove two values of the same type from the stackm, returning a single <see cref="ValueType.Int32"/>.
	/// </summary>
	public abstract class ValueTwoToInt32NotEqualZeroInstruction : ValueTwoToInt32Instruction
	{
		internal ValueTwoToInt32NotEqualZeroInstruction()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			base.Compile(context);

			context.Emit(OpCodes.Ldc_I4_0);
			context.Emit(OpCodes.Ceq);
		}
	}
}
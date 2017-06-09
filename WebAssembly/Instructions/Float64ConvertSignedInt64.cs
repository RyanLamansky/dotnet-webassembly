using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert a signed 64-bit integer to a 64-bit float.
	/// </summary>
	public class Float64ConvertSignedInt64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ConvertSignedInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ConvertSignedInt64;

		/// <summary>
		/// Creates a new  <see cref="Float64ConvertSignedInt64"/> instance.
		/// </summary>
		public Float64ConvertSignedInt64()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(OpCode.Float64ConvertSignedInt64, 1, 0);

			var type = stack.Pop();
			if (type != ValueType.Int64)
				throw new StackTypeInvalidException(OpCode.Float64ConvertSignedInt64, ValueType.Int64, type);

			context.Emit(OpCodes.Conv_R8);

			stack.Push(ValueType.Float64);
		}
	}
}
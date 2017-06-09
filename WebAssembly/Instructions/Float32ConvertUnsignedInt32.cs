using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert an unsigned 32-bit integer to a 32-bit float.
	/// </summary>
	public class Float32ConvertUnsignedInt32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ConvertUnsignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ConvertUnsignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Float32ConvertUnsignedInt32"/> instance.
		/// </summary>
		public Float32ConvertUnsignedInt32()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(OpCode.Float32ConvertUnsignedInt32, 1, 0);

			var type = stack.Pop();
			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(OpCode.Float32ConvertUnsignedInt32, ValueType.Int32, type);

			context.Emit(OpCodes.Conv_R_Un);
			context.Emit(OpCodes.Conv_R4);

			stack.Push(ValueType.Float32);
		}
	}
}
using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert a signed 32-bit integer to a 32-bit float.
	/// </summary>
	public class Float32ConvertSignedInt32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ConvertSignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ConvertSignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Float32ConvertSignedInt32"/> instance.
		/// </summary>
		public Float32ConvertSignedInt32()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(OpCode.Float32ConvertSignedInt32, 1, 0);

			var type = stack.Pop();
			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(OpCode.Float32ConvertSignedInt32, ValueType.Int32, type);

			context.Emit(OpCodes.Conv_R4);

			stack.Push(ValueType.Float32);
		}
	}
}
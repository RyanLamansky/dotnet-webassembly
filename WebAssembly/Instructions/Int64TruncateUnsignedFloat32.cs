using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 32-bit float to an unsigned 64-bit integer.
	/// </summary>
	public class Int64TruncateUnsignedFloat32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64TruncateUnsignedFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64TruncateUnsignedFloat32;

		/// <summary>
		/// Creates a new  <see cref="Int64TruncateUnsignedFloat32"/> instance.
		/// </summary>
		public Int64TruncateUnsignedFloat32()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(OpCode.Int64TruncateUnsignedFloat32, 1, 0);

			var type = stack.Pop();
			if (type != ValueType.Float32)
				throw new StackTypeInvalidException(OpCode.Int64TruncateUnsignedFloat32, ValueType.Float32, type);

			context.Emit(OpCodes.Conv_Ovf_I8_Un);

			stack.Push(ValueType.Int64);
		}
	}
}
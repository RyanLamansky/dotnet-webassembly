using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Extend an unsigned 32-bit integer to a 64-bit integer.
	/// </summary>
	public class Int64ExtendUnsignedInt32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ExtendUnsignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ExtendUnsignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Int64ExtendUnsignedInt32"/> instance.
		/// </summary>
		public Int64ExtendUnsignedInt32()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 1)
				throw new StackTooSmallException(this.OpCode, 1, stack.Count);

			var type = stack.Pop();

			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(this.OpCode, ValueType.Int32, type);

			context.Emit(OpCodes.Conv_U8);

			stack.Push(ValueType.Int64);
		}
	}
}
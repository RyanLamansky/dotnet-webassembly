using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Promote a 32-bit float to a 64-bit float.
	/// </summary>
	public class Float64PromoteFloat32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64PromoteFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64PromoteFloat32;

		/// <summary>
		/// Creates a new  <see cref="Float64PromoteFloat32"/> instance.
		/// </summary>
		public Float64PromoteFloat32()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(OpCode.Float64PromoteFloat32, 1, 0);

			var type = stack.Pop();
			if (type != ValueType.Float32)
				throw new StackTypeInvalidException(OpCode.Float64PromoteFloat32, ValueType.Float32, type);

			context.Emit(OpCodes.Conv_R8);

			stack.Push(ValueType.Float64);
		}
	}
}
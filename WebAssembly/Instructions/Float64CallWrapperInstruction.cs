using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wraps a <see cref="double"/>-only .NET API call with conversions so it can be used with <see cref="ValueType.Float32"/>.
	/// </summary>
	public abstract class Float64CallWrapperInstruction : SimpleInstruction
	{
		private protected Float64CallWrapperInstruction()
		{
		}

		private protected abstract MethodInfo MethodInfo { get; }

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(this.OpCode, 1, 0);

			var type = stack.Peek();  //Assuming validation passes, the remaining type will be this.
			if (type != ValueType.Float32)
				throw new StackTypeInvalidException(this.OpCode, ValueType.Float32, type);

			context.Emit(OpCodes.Conv_R8);
			context.Emit(OpCodes.Call, this.MethodInfo);
			context.Emit(OpCodes.Conv_R4);
		}
	}
}
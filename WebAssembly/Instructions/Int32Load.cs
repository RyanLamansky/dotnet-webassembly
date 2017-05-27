using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 4 bytes as i32.
	/// </summary>
	public class Int32Load : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load;

		/// <summary>
		/// Creates a new  <see cref="Int32Load"/> instance.
		/// </summary>
		public Int32Load()
		{
		}

		internal Int32Load(Reader reader)
			: base(reader)
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(OpCode.Int32Load, 1, 0);

			var type = stack.Peek();  //Assuming validation passes, the remaining type will be this.
			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(OpCode.Int32Load, ValueType.Int32, type);

			if (this.Offset != 0)
			{
				Int32Constant.Emit(context, (int)this.Offset);
				context.Emit(OpCodes.Add_Ovf_Un);
			}

			context.Emit(OpCodes.Ldarg_0);
			context.Emit(OpCodes.Call, context[HelperMethod.RangeCheckInt32]);

			context.Emit(OpCodes.Ldarg_0);
			context.Emit(OpCodes.Ldfld, context.LinearMemoryStart);
			context.Emit(OpCodes.Add);
			context.Emit(OpCodes.Ldind_I4);
		}
	}
}
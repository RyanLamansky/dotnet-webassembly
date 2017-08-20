using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// (i32 index, T value){T} => i32 index; Write a global variable.
	/// </summary>
	public class SetGlobal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.SetGlobal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.SetGlobal;

		/// <summary>
		/// Creates a new  <see cref="SetGlobal"/> instance.
		/// </summary>
		public SetGlobal()
		{
		}

		/// <summary>
		/// Creates a new <see cref="SetGlobal"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		public SetGlobal(uint index)
			: base(index)
		{
		}

		internal SetGlobal(Reader reader)
			: base(reader)
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 1)
				throw new StackTooSmallException(OpCode.SetGlobal, 1, stack.Count);

			Compile.GlobalInfo setter;
			try
			{
				setter = context.GlobalSetters[this.Index];
			}
			catch (System.IndexOutOfRangeException x)
			{
				throw new CompilerException($"Global at index {this.Index} does not exist.", x);
			}

			if (setter == null)
				throw new CompilerException($"Global at index {this.Index} is immutable.");

			var type = stack.Pop();
			if (type != setter.Type)
				throw new StackTypeInvalidException(OpCode.SetGlobal, setter.Type, type);

			context.EmitLoadThis();
			context.Emit(OpCodes.Call, setter.Builder);
		}
	}
}
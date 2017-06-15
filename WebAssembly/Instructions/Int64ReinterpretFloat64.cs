using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Reinterpret the bits of a 64-bit float as a 64-bit integer.
	/// </summary>
	public class Int64ReinterpretFloat64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ReinterpretFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ReinterpretFloat64;

		/// <summary>
		/// Creates a new  <see cref="Int64ReinterpretFloat64"/> instance.
		/// </summary>
		public Int64ReinterpretFloat64()
		{
		}

		internal override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 1)
				throw new StackTooSmallException(OpCode.Int64ReinterpretFloat64, 1, stack.Count);

			var type = stack.Pop();
			if (type != ValueType.Float64)
				throw new StackTypeInvalidException(OpCode.Int64ReinterpretFloat64, ValueType.Float64, type);

			stack.Push(ValueType.Int64);

			context.Emit(OpCodes.Call, context[HelperMethod.Int64ReinterpretFloat64, (helper, exportsBuilder) =>
			{
				var builder = exportsBuilder.DefineMethod(
					"☣ Int64ReinterpretFloat64",
					CompilationContext.HelperMethodAttributes,
					typeof(long),
					new[]
					{
							typeof(double),
					}
					);

				var il = builder.GetILGenerator();
				il.Emit(OpCodes.Ldarga_S, 0);
				il.Emit(OpCodes.Ldind_I8);
				il.Emit(OpCodes.Ret);
				return builder;
			}
			]);
		}
	}
}
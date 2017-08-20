using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// A ternary operator with two operands, which have the same type as each other, plus a boolean (i32) condition. Returns the first operand if the condition operand is non-zero, or the second otherwise.
	/// </summary>
	public class Select : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Select"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Select;

		/// <summary>
		/// Creates a new  <see cref="Select"/> instance.
		/// </summary>
		public Select()
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			Assert(context != null);

			var stack = context.Stack;
			Assert(stack != null);
			if (stack.Count < 3)
				throw new StackTooSmallException(OpCode.Select, 3, stack.Count);

			var type = stack.Pop();
			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(OpCode.Select, ValueType.Int32, type);

			var typeB = stack.Pop();
			var typeA = stack.Peek(); //Assuming validation passes, the remaining type will be this.

			if (typeA != typeB)
				throw new StackParameterMismatchException(OpCode.Select, typeA, typeB);

			HelperMethod helper;
			switch (typeA)
			{
				default: //This shouldn't be possible due to previous validations.
					Fail("Unknown ValueType.");
					return;
				case ValueType.Int32: helper = HelperMethod.SelectInt32; break;
				case ValueType.Int64: helper = HelperMethod.SelectInt64; break;
				case ValueType.Float32: helper = HelperMethod.SelectFloat32; break;
				case ValueType.Float64: helper = HelperMethod.SelectFloat64; break;
			}
			context.Emit(OpCodes.Call, context[helper, CreateSelectHelper]);
		}

		static MethodBuilder CreateSelectHelper(HelperMethod helper, CompilationContext context)
		{
			Assert(context != null);

			MethodBuilder builder;
			switch (helper)
			{
				default:
					Fail("Attempted to obtain an unknown helper method.");
					return null;
				case HelperMethod.SelectInt32:
					builder = context.ExportsBuilder.DefineMethod(
						"☣ Select Int32",
						CompilationContext.HelperMethodAttributes,
						typeof(int),
						new[]
						{
								typeof(int),
								typeof(int),
								typeof(int),
						}
						);
					break;

				case HelperMethod.SelectInt64:
					builder = context.ExportsBuilder.DefineMethod(
						"☣ Select Int64",
						CompilationContext.HelperMethodAttributes,
						typeof(long),
						new[]
						{
								typeof(long),
								typeof(long),
								typeof(int),
						}
						);
					break;

				case HelperMethod.SelectFloat32:
					builder = context.ExportsBuilder.DefineMethod(
						"☣ Select Float32",
						CompilationContext.HelperMethodAttributes,
						typeof(float),
						new[]
						{
								typeof(float),
								typeof(float),
								typeof(int),
						}
						);
					break;

				case HelperMethod.SelectFloat64:
					builder = context.ExportsBuilder.DefineMethod(
						"☣ Select Float64",
						CompilationContext.HelperMethodAttributes,
						typeof(double),
						new[]
						{
								typeof(double),
								typeof(double),
								typeof(int),
						}
						);
					break;
			}

			var il = builder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_2);
			var @true = il.DefineLabel();
			il.Emit(OpCodes.Brtrue_S, @true);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ret);
			il.MarkLabel(@true);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ret);
			return builder;
		}
	}
}
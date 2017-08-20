using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Like <see cref="SetLocal"/>, but also returns the set value.
	/// </summary>
	public class TeeLocal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.TeeLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.TeeLocal;

		/// <summary>
		/// Creates a new  <see cref="TeeLocal"/> instance.
		/// </summary>
		public TeeLocal()
		{
		}

		/// <summary>
		/// Creates a new <see cref="TeeLocal"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		public TeeLocal(uint index)
			: base(index)
		{
		}

		internal TeeLocal(Reader reader)
			: base(reader)
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count < 1)
				throw new StackTooSmallException(OpCode.TeeLocal, 1, stack.Count);

			var setType = stack.Peek();  //Assuming validation passes, the remaining type will be this.
			if (setType != context.Locals[this.Index])
				throw new StackTypeInvalidException(OpCode.TeeLocal, context.Locals[this.Index], setType);

			context.Emit(OpCodes.Dup);

			var localIndex = this.Index - context.Signature.ParameterTypes.Length;
			if (localIndex < 0)
			{
				//Referring to a parameter.
				if (this.Index <= byte.MaxValue)
					context.Emit(OpCodes.Starg_S, checked((byte)this.Index));
				else
					context.Emit(OpCodes.Starg, checked((ushort)this.Index));
			}
			else
			{
				//Referring to a local.
				switch (localIndex)
				{
					default:
						if (localIndex > 65534) // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc
							throw new CompilerException($"Implementation limit exceeded: maximum accessible local index is 65534, tried to access {localIndex}.");

						if (localIndex <= byte.MaxValue)
							context.Emit(OpCodes.Stloc_S, (byte)localIndex);
						else
							context.Emit(OpCodes.Stloc, checked((ushort)localIndex));
						break;

					case 0: context.Emit(OpCodes.Stloc_0); break;
					case 1: context.Emit(OpCodes.Stloc_1); break;
					case 2: context.Emit(OpCodes.Stloc_2); break;
					case 3: context.Emit(OpCodes.Stloc_3); break;
				}
			}
		}
	}
}
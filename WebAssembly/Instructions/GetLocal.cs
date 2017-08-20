using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Read the current value of a local variable.
	/// </summary>
	public class GetLocal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GetLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GetLocal;

		/// <summary>
		/// Creates a new  <see cref="GetLocal"/> instance.
		/// </summary>
		public GetLocal()
		{
		}

		/// <summary>
		/// Creates a new <see cref="GetLocal"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		public GetLocal(uint index)
			: base(index)
		{
		}

		internal GetLocal(Reader reader)
			: base(reader)
		{
		}

		internal sealed override void Compile(CompilationContext context)
		{
			context.Stack.Push(context.Locals[this.Index]);

			var localIndex = this.Index - context.Signature.ParameterTypes.Length;
			if (localIndex < 0)
			{
				//Referring to a parameter.
				switch (this.Index)
				{
					default:
						if (this.Index <= byte.MaxValue)
							context.Emit(OpCodes.Ldarg_S, checked((byte)this.Index));
						else
							context.Emit(OpCodes.Ldarg, checked((ushort)this.Index));
						break;

					case 0: context.Emit(OpCodes.Ldarg_0); break;
					case 1: context.Emit(OpCodes.Ldarg_1); break;
					case 2: context.Emit(OpCodes.Ldarg_2); break;
					case 3: context.Emit(OpCodes.Ldarg_3); break;
				}
			}
			else
			{
				//Referring to a local.
				switch (localIndex)
				{
					default:
						if (localIndex > 65534) // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc
							throw new CompilerException($"Implementation limit exceeded: maximum accessible local index is 65534, tried to access {localIndex}.");

						if (localIndex <= byte.MaxValue)
							context.Emit(OpCodes.Ldloc_S, (byte)localIndex);
						else
							context.Emit(OpCodes.Ldloc, checked((ushort)localIndex));
						break;

					case 0: context.Emit(OpCodes.Ldloc_0); break;
					case 1: context.Emit(OpCodes.Ldloc_1); break;
					case 2: context.Emit(OpCodes.Ldloc_2); break;
					case 3: context.Emit(OpCodes.Ldloc_3); break;
				}
			}
		}
	}
}
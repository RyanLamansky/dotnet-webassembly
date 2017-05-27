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

		internal override void Compile(CompilationContext context)
		{
			var localIndex = this.Index - context.Function.Signature.param_types.Length;
			if (localIndex < 0)
			{
				//Referring to a parameter.
				//Argument 0 is for the "this" parameter, allowing access to features unique to the WASM instance.
				switch (this.Index)
				{
					default:
						context.Emit(OpCodes.Ldarg, checked((ushort)(this.Index + 1)));
						break;

					case 0: context.Emit(OpCodes.Ldarg_1); break;
					case 1: context.Emit(OpCodes.Ldarg_2); break;
					case 2: context.Emit(OpCodes.Ldarg_3); break;
				}
			}
			else
			{
				//Referring to a local.
				switch (localIndex)
				{
					default:
						context.Emit(OpCodes.Ldloc, checked((int)localIndex));
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
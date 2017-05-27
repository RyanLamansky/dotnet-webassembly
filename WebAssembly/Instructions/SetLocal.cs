using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Set the current value of a local variable.
	/// </summary>
	public class SetLocal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.SetLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.SetLocal;

		/// <summary>
		/// Creates a new  <see cref="SetLocal"/> instance.
		/// </summary>
		public SetLocal()
		{
		}

		/// <summary>
		/// Creates a new <see cref="SetLocal"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		public SetLocal(uint index)
			: base(index)
		{
		}

		internal SetLocal(Reader reader)
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
				if (this.Index + 1 <= byte.MaxValue)
					context.Emit(OpCodes.Starg_S, checked((byte)(this.Index + 1)));
				else
					context.Emit(OpCodes.Starg, checked((ushort)(this.Index + 1)));
			}
			else
			{
				//Referring to a local.
				switch (localIndex)
				{
					default:
						context.Emit(OpCodes.Stloc, checked((int)localIndex));
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
namespace WebAssembly.Instructions
{
	/// <summary>
	/// Simple instructions have no customizability; their <see cref="OpCode"/> defines their entire action.
	/// </summary>
    public abstract class SimpleInstruction : Instruction
    {
		internal SimpleInstruction()
		{
		}

		internal sealed override void WriteTo(Writer writer)
		{
			writer.Write((byte)this.OpCode);
		}
	}
}

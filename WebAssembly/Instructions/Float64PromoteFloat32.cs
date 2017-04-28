namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64PromoteFloat32.
	/// </summary>
	public class Float64PromoteFloat32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64PromoteFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64PromoteFloat32;

		/// <summary>
		/// Creates a new  <see cref="Float64PromoteFloat32"/> instance.
		/// </summary>
		public Float64PromoteFloat32()
		{
		}
	}
}
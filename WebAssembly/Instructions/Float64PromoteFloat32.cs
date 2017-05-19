namespace WebAssembly.Instructions
{
	/// <summary>
	/// Promote a 32-bit float to a 64-bit float.
	/// </summary>
	public class Float64PromoteFloat32 : SimpleInstruction
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
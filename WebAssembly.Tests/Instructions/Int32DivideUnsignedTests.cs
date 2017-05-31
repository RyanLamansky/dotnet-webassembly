using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32DivideUnsigned"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32DivideUnsignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32Add"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32DivideUnsigned_Compiled()
		{
			const uint divisor = 2;

			var exports = AssemblyBuilder.CreateInstance<CompilerTestBase<int>>(nameof(CompilerTestBase<int>.Test),
				ValueType.Int32,
				 new[]
				 {
					 ValueType.Int32
				 },
				new GetLocal(0),
				new Int32Constant(divisor),
				new Int32DivideUnsigned(),
				new End());

			foreach (var value in new uint[] { 0, 1, 2, 3, 4, 5, })
				Assert.AreEqual(value / divisor, (uint)exports.Test((int)value));
		}
	}
}
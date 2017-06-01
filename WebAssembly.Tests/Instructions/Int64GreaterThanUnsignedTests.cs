using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64GreaterThanUnsigned"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64GreaterThanUnsignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64GreaterThanUnsigned"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64GreaterThanUnsigned_Compiled()
		{
			const uint comparand = 0xF;

			var exports = CompilerTestBase<long>.CreateInstance(
				new GetLocal(0),
				new Int64Constant(comparand),
				new Int64GreaterThanUnsigned(),
				new End());

			foreach (var value in new ulong[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value > comparand, exports.Test((long)value) == 1);
		}
	}
}
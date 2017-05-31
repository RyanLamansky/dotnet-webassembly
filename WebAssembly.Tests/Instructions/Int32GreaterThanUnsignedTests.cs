using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32GreaterThanUnsigned"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32GreaterThanUnsignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32GreaterThanUnsigned"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32GreaterThanUnsigned_Compiled()
		{
			const uint comparand = 0xF;

			var exports = CompilerTestBase<int>.CreateInstance(
				new GetLocal(0),
				new Int32Constant(comparand),
				new Int32GreaterThanUnsigned(),
				new End());

			foreach (var value in new uint[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value > comparand, exports.Test((int)value) == 1);
		}
	}
}
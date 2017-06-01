using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Subtract"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64SubtractTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Subtract"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Subtract_Compiled()
		{
			const int comparand = 0x8;

			var exports = CompilerTestBase<long>.CreateInstance(
				new GetLocal(0),
				new Int64Constant(comparand),
				new Int64Subtract(),
				new End());

			foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value - comparand, exports.Test(value));
		}
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32Subtract"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32SubtractTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32Subtract"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32Subtract_Compiled()
		{
			const int comparand = 0x8;

			var exports = CompilerTestBase<int>.CreateInstance(
				new GetLocal(0),
				new Int32Constant(comparand),
				new Int32Subtract(),
				new End());

			foreach (var value in new[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value - comparand, exports.Test(value));
		}
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32Equal"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32EqualTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32Equal"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32Equal_Compiled()
		{
			const int target = 0xF;

			var exports = CompilerTestBase<int>.CreateInstance(
				new GetLocal(0),
				new Int32Constant(target),
				new Int32Equal(),
				new End());

			foreach (var value in new[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value == target, exports.Test(value) == 1);
		}
	}
}
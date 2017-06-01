using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Equal"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64EqualTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Equal"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Equal_Compiled()
		{
			const int target = 0xF;

			var exports = CompilerTestBase<long>.CreateInstance(
				new GetLocal(0),
				new Int64Constant(target),
				new Int64Equal(),
				new End());

			foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value == target, exports.Test(value) == 1);
		}
	}
}
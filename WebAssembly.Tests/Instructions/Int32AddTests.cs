using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32Add"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32AddTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32Add"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32Add_Compiled()
		{
			var exports = CompilerTestBase<int>.CreateInstance(
				new GetLocal(0),
				new Int32Constant(1),
				new Int32Add(),
				new End());

			Assert.AreEqual(1, exports.Test(0));
			Assert.AreEqual(6, exports.Test(5));
		}
	}
}
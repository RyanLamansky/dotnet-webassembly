using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Add"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64AddTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Add"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Add_Compiled()
		{
			var exports = CompilerTestBase<long>.CreateInstance(
				new GetLocal(0),
				new Int64Constant(1),
				new Int64Add(),
				new End());

			Assert.AreEqual(1, exports.Test(0));
			Assert.AreEqual(6, exports.Test(5));
		}
	}
}
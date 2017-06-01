using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32Add"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32AddTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32Add"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Add_Compiled()
		{
			var exports = CompilerTestBase<float>.CreateInstance(
				new GetLocal(0),
				new Float32Constant(1),
				new Float32Add(),
				new End());

			Assert.AreEqual(1, exports.Test(0));
			Assert.AreEqual(6, exports.Test(5));
		}
	}
}
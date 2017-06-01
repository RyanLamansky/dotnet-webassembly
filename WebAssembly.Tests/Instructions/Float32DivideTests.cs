using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32Divide"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32DivideTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32Divide"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Divide_Compiled()
		{
			var exports = CompilerTestBase<float>.CreateInstance(
				new GetLocal(0),
				new Float32Constant(3),
				new Float32Divide(),
				new End());

			Assert.AreEqual(0, exports.Test(0));
			Assert.AreEqual(3, exports.Test(9));
			Assert.AreEqual(-2, exports.Test(-6));
		}
	}
}
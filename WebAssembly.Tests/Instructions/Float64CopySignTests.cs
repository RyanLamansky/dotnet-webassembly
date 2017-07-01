using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float64CopySign"/> instruction.
	/// </summary>
	[TestClass]
	public class Float64CopySignTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float64CopySign"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float64CopySign_Compiled()
		{
			var exports = CompilerTestBase2<double>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Float64CopySign(),
				new End());

			Assert.AreEqual(1, exports.Test(1, +2));
			Assert.AreEqual(-1, exports.Test(1, -2));
			Assert.AreEqual(-double.PositiveInfinity, exports.Test(double.PositiveInfinity, -2));
			Assert.AreEqual(-double.NaN, exports.Test(double.NaN, -2));
		}
	}
}
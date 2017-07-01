using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32Negate"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32NegateTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32Negate"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Negate_Compiled()
		{
			var exports = CompilerTestBase<float>.CreateInstance(
				new GetLocal(0),
				new Float32Negate(),
				new End());

			foreach (var value in Samples.Single)
				Assert.AreEqual(-value, exports.Test(value));
		}
	}
}
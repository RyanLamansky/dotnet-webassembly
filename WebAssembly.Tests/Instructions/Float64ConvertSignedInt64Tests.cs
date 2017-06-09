using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float64ConvertSignedInt64"/> instruction.
	/// </summary>
	[TestClass]
	public class Float64ConvertSignedInt64Tests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float64ConvertSignedInt64"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float64ConvertSignedInt64_Compiled()
		{
			var exports = ConversionTestBase<long, double>.CreateInstance(
				new GetLocal(0),
				new Float64ConvertSignedInt64(),
				new End());

			foreach (var value in Samples.Int64)
				Assert.AreEqual(value, exports.Test(value));
		}
	}
}
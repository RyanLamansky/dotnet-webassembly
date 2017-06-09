using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32ConvertSignedInt64"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32ConvertSignedInt64Tests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32ConvertSignedInt64"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32ConvertSignedInt64_Compiled()
		{
			var exports = ConversionTestBase<long, float>.CreateInstance(
				new GetLocal(0),
				new Float32ConvertSignedInt64(),
				new End());

			foreach (var value in Samples.Int64)
				Assert.AreEqual(value, exports.Test(value));
		}
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64TruncateUnsignedFloat64"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64TruncateUnsignedFloat64Tests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64TruncateUnsignedFloat64"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64TruncateUnsignedFloat64_Compiled()
		{
			var exports = ConversionTestBase<double, long>.CreateInstance(
				new GetLocal(0),
				new Int64TruncateUnsignedFloat64(),
				new End());

			foreach (var value in new[] { 0, 1.5, -1.5, 123445678901234.0 })
				Assert.AreEqual((long)value, exports.Test(value));

			const double exceptional = 1234456789012345678901234567890.0;
			ExceptionAssert.Expect<System.OverflowException>(() => exports.Test(exceptional));
		}
	}
}
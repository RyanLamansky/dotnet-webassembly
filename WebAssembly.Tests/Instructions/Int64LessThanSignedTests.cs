using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64LessThanSigned"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64LessThanSignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64LessThanSigned"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64LessThanSigned_Compiled()
		{
			const int comparand = 0xF;

			var exports = CompilerTestBase<long>.CreateInstance(
				new GetLocal(0),
				new Int64Constant(comparand),
				new Int64LessThanSigned(),
				new End());

			foreach (var value in new long[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value < comparand, exports.Test(value) == 1);
		}
	}
}
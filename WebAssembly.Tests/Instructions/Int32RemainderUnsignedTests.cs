using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32RemainderUnsigned"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32RemainderUnsignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32RemainderUnsigned"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32RemainderUnsigned_Compiled()
		{
			const uint divisor = 0xF;

			var exports = CompilerTestBase<int>.CreateInstance(
				new GetLocal(0),
				new Int32Constant(divisor),
				new Int32RemainderUnsigned(),
				new End());

			foreach (var value in new uint[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value % divisor, (uint)exports.Test((int)value));
		}
	}
}
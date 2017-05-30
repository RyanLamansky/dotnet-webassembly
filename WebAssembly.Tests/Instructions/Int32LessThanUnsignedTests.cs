using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32LessThanUnsigned"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32LessThanUnsignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32LessThanUnsigned"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32LessThanUnsigned_Compiled()
		{
			const uint comparand = 0xF;

			var exports = AssemblyBuilder.CreateInstance<CompilerTestBase>(nameof(CompilerTestBase.Test),
				ValueType.Int32,
				 new[]
				 {
					 ValueType.Int32
				 },
				new GetLocal(0),
				new Int32Constant(comparand),
				new Int32LessThanUnsigned(),
				new End());

			foreach (var value in new uint[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value < comparand, exports.Test((int)value) == 1);
		}
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32And"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32AndTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32And"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32And_Compiled()
		{
			const int and = 0xF;

			var exports = AssemblyBuilder.CreateInstance<CompilerTestBase>(nameof(CompilerTestBase.Test),
				ValueType.Int32,
				 new[]
				 {
					 ValueType.Int32
				 },
				new GetLocal(0),
				new Int32Constant(and),
				new Int32And(),
				new End());

			foreach (var value in new[] { 0x00, 0x0F, 0xF0, 0xFF, })
				Assert.AreEqual(value & and, exports.Test(value));
		}
	}
}
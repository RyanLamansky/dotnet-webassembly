using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="If"/> instruction.
	/// </summary>
	[TestClass]
	public class IfTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="If"/> instruction.
		/// </summary>
		[TestMethod]
		public void If_Compiled()
		{
			var exports = AssemblyBuilder.CreateInstance<CompilerTestBase>(nameof(CompilerTestBase.Test),
				ValueType.Int32,
				 new[]
				 {
					 ValueType.Int32
				 },
				new GetLocal(0),
				new If(),
				new Int32Constant(3),
				new Return(),
				new End(),
				new Int32Constant(2),
				new End());

			Assert.AreEqual(2, exports.Test(0));
			Assert.AreEqual(3, exports.Test(1));
		}
	}
}
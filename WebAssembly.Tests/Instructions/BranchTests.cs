using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Branch"/> instruction.
	/// </summary>
	[TestClass]
	public class BranchTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Branch"/> instruction.
		/// </summary>
		[TestMethod]
		public void Branch_Compiled()
		{
			var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
				ValueType.Int32,
				new Block(BlockType.Empty),
				new Block(BlockType.Empty),
				new Block(BlockType.Empty),
				new End(),
				new Block(BlockType.Empty),
				new Branch(1),
				new End(),
				new End(),
				new Int32Constant(2),
				new Return(),
				new End(),
				new Int32Constant(1),
				new End());

			Assert.AreEqual<int>(2, exports.Test());
		}
	}
}
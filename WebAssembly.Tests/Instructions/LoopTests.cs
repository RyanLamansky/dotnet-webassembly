using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Loop"/> instruction.
	/// </summary>
	[TestClass]
	public class LoopTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Loop"/> instruction.
		/// </summary>
		[TestMethod]
		[Timeout(1000)]
		public void Loop_Compiled()
		{
			var exports = AssemblyBuilder.CreateInstance<CompilerTestBase2>(nameof(CompilerTestBase2.Test),
				ValueType.Int32,
				new[]
				{
					ValueType.Int32,
					ValueType.Int32,
				},
				new Block(BlockType.Empty),
				new Loop(BlockType.Empty),

				new GetLocal(0),
				new Int32Constant(1),
				new Int32Add(),
				new SetLocal(0),

				new GetLocal(1),
				new Int32Constant(1),
				new Int32Add(),
				new SetLocal(1),

				new GetLocal(1),
				new If(BlockType.Empty),
				new Branch(2),
				new Else(),
				new Branch(1),
				new End(), //if

				new End(), //loop
				new End(), //block
				new GetLocal(0),
				new End());

			Assert.AreEqual(11, exports.Test(10, -2));
			Assert.AreEqual(12, exports.Test(10, -1));
			Assert.AreEqual(11, exports.Test(10, 0));
		}
	}
}
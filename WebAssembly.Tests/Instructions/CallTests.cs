using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Call"/> instruction.
	/// </summary>
	[TestClass]
	public class CallTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Call"/> instruction.
		/// </summary>
		[TestMethod]
		public void Call_Compiled()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Parameters = new[]
				{
					ValueType.Int32,
				},
				Returns = new[]
				{
					ValueType.Int32,
				}
			});
			module.Functions.Add(new Function
			{
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(CompilerTestBase<int>.Test)
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
					new GetLocal(0),
					new Call(1),
					new End(),
				},
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
					new GetLocal(0),
					new Int32Constant(1),
					new Int32Add(),
					new End(),
				},
			});

			Instance<CompilerTestBase<int>> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<CompilerTestBase<int>>(memory)();
			}

			var exports = compiled.Exports;
			Assert.AreEqual(1, exports.Test(0));
			Assert.AreEqual(4, exports.Test(3));
		}
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="CallIndirect"/> instruction.
	/// </summary>
	[TestClass]
	public class CallIndirectTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="CallIndirect"/> instruction.
		/// </summary>
		[TestMethod]
		public void CallIndirect_Compiled()
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
			module.Types.Add(new Type
			{
				Parameters = new ValueType[]
				{
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
				Type = 1,
			});
			module.Functions.Add(new Function
			{
				Type = 1,
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
					new CallIndirect(1),
					new End(),
				},
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
					new Int32Constant(5),
					new End(),
				},
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				 {
					new Int32Constant(6),
					new End(),
				 },
			});
			module.Tables.Add(new Table
			{
				ElementType = ElementType.AnyFunction,
				ResizableLimits = new ResizableLimits(2, 2),
			});
			module.Elements.Add(new Element
			{
				Elements = new uint[] { 1, 2 },
				InitializerExpression = new Instruction[]
				{
					new Int32Constant(0),
					new End(),
				},
			});

			var compiled = module.ToInstance<CompilerTestBase<int>>();

			var exports = compiled.Exports;
			Assert.AreEqual(5, exports.Test(0));
			Assert.AreEqual(6, exports.Test(1));
		}
	}
}
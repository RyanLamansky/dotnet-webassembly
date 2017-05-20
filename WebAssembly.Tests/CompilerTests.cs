using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace WebAssembly
{
	using Compiled;
	using Instructions;

	/// <summary>
	/// Validates basic features of the <see cref="Compiler"/> class.
	/// </summary>
	[TestClass]
	public class CompilerTests
	{
		/// <summary>
		/// Tests a very simple program with a single exported function that returns a number.
		/// </summary>
		[TestMethod]
		public void Compile_HelloWorld()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = "Start"
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new Int32Constant { Value = 3 },
				new End(),
				},
			});

			Instance compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compiler.FromBinary(memory)();
			}

			dynamic exports = compiled.Exports;
			Assert.AreEqual(3, (int)exports.Start());
		}
	}
}
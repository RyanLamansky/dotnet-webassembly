using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace WebAssembly
{
	using Instructions;

	/// <summary>
	/// Tests basic functionality of <see cref="RuntimeImport"/> when used with <see cref="Compile"/>.
	/// </summary>
	[TestClass]
	public class RuntimeImportTests
	{
		/// <summary>
		/// Verifies that <see cref="RuntimeImport"/> when used with <see cref="Compile"/> work properly together.
		/// </summary>
		[TestMethod]
		[Timeout(1000)]
		public void Compile_RuntimeImport()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Returns = new[] { ValueType.Float64 },
				Parameters = new[] { ValueType.Float64, ValueType.Float64, }
			});
			module.Imports.Add(new Import.Function { Module = "Math", Field = "Pow", });
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = "Test",
				Index = 1,
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new GetLocal(0),
				new GetLocal(1),
				new Call(0),
				new End()
				},
			});

			Instance<CompilerTestBase2<double>> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				Assert.AreNotEqual(0, memory.Length);
				memory.Position = 0;

				var maker = Compile.FromBinary<CompilerTestBase2<double>>(memory,
					new RuntimeImport[] {
					new FunctionImport("Math", "Pow", typeof(Math).GetTypeInfo().GetMethod("Pow"))
					});
				Assert.IsNotNull(maker);
				compiled = maker();
			}

			Assert.IsNotNull(compiled);
			Assert.IsNotNull(compiled.Exports);

			var instance = compiled.Exports;

			Assert.AreEqual(Math.Pow(2, 3), instance.Test(2, 3));
		}
	}
}

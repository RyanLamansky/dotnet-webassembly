using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

			var compiled = module.ToInstance<CompilerTestBase2<double>>(
				new RuntimeImport[] {
					new FunctionImport("Math", "Pow", typeof(Math).GetTypeInfo().GetMethod("Pow"))
				});

			Assert.IsNotNull(compiled);
			Assert.IsNotNull(compiled.Exports);

			var instance = compiled.Exports;

			Assert.AreEqual(Math.Pow(2, 3), instance.Test(2, 3));
		}

		/// <summary>
		/// Used by <see cref="Compile_RuntimeImportNoReturn"/>.
		/// </summary>
		public static class NothingDoer
		{
			/// <summary>
			/// The number of calls to <see cref="DoNothing"/> made.
			/// </summary>
			public static int Calls;

			/// <summary>
			/// Does nothing.
			/// </summary>
			/// <param name="ignored">Ignored.</param>
			public static void DoNothing(double ignored) => System.Threading.Interlocked.Increment(ref Calls);
		}

		/// <summary>
		/// Verifies that <see cref="RuntimeImport"/> when used with <see cref="Compile"/> work properly together.
		/// </summary>
		[TestMethod]
		[Timeout(1000)]
		public void Compile_RuntimeImportNoReturn()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Parameters = new[] { ValueType.Float64, }
			});
			module.Imports.Add(new Import.Function { Module = "Do", Field = "Nothing", });
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
				new Call(0),
				new End()
				},
			});

			var compiled = module.ToInstance<CompilerTestBaseVoid<double>>(
				new RuntimeImport[] {
					new FunctionImport("Do", "Nothing", typeof(NothingDoer).GetTypeInfo().GetMethod(nameof(NothingDoer.DoNothing)))
				});

			Assert.IsNotNull(compiled);
			Assert.IsNotNull(compiled.Exports);

			var instance = compiled.Exports;

			lock (typeof(NothingDoer))
			{
				var start = NothingDoer.Calls;
				instance.Test(2);
				Assert.AreEqual(start + 1, NothingDoer.Calls);
			}
		}
	}
}
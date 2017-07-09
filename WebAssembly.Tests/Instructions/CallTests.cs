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

		/// <summary>
		/// Used by <see cref="Call_Compiled_MixedParameterTypes"/>.
		/// </summary>
		public abstract class MixedParameters
		{
			/// <summary>
			/// Returns a value.
			/// </summary>
			/// <param name="parameter1">Input to the test function.</param>
			/// <param name="parameter2">Input to the test function.</param>
			/// <returns>A value to ensure proper control flow and execution.</returns>
			public abstract int Test(int parameter1, double parameter2);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Call"/> instruction with mixed parameter types.
		/// </summary>
		[TestMethod]
		public void Call_Compiled_MixedParameterTypes()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Parameters = new[]
				{
					ValueType.Int32,
					ValueType.Float64, //Not actually used, just here to verify that the call works correctly.
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
				Name = nameof(MixedParameters.Test)
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
					new GetLocal(0),
					new GetLocal(1),
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

			Instance<MixedParameters> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<MixedParameters>(memory)();
			}

			var exports = compiled.Exports;
			Assert.AreEqual(1, exports.Test(0, 0));
			Assert.AreEqual(4, exports.Test(3, 3));
		}
	}
}
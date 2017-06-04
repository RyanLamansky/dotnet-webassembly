using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Select"/> instruction.
	/// </summary>
	[TestClass]
	public class SelectTests
	{
		/// <summary>
		/// Tests the <see cref="Select"/> instruction.
		/// </summary>
		/// <typeparam name="T">The value to be returned.</typeparam>
		public abstract class SelectTester<T>
			where T : struct
		{
			/// <summary>
			/// Tests the <see cref="Select"/> instruction.
			/// </summary>
			public abstract T Test(T a, T b, int c);
		}

		static Instance<SelectTester<T>> CreateTester<T>(ValueType type)
			where T : struct
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Parameters = new ValueType[]
				{
					type,
					type,
					ValueType.Int32,
				},
				Returns = new[]
				{
					type,
				},
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(SelectTester<T>.Test),
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
					new GetLocal(0),
					new GetLocal(1),
					new GetLocal(2),
					new Select(),
					new End(),
				},
			});

			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				return Compile.FromBinary<SelectTester<T>>(memory)();
			}
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="ValueType.Int32"/> input.
		/// </summary>
		[TestMethod]
		public void Select_Compiled_Int32()
		{
			using (var compiled = CreateTester<int>(ValueType.Int32))
			{
				var exports = compiled.Exports;
				Assert.AreEqual(1, exports.Test(1, 2, 3));
				Assert.AreEqual(2, exports.Test(1, 2, 0));
				Assert.AreEqual(4, exports.Test(4, 5, 1));
				Assert.AreEqual(5, exports.Test(4, 5, 0));
			}
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="ValueType.Int64"/> input.
		/// </summary>
		[TestMethod]
		public void Select_Compiled_Int64()
		{
			using (var compiled = CreateTester<long>(ValueType.Int64))
			{
				var exports = compiled.Exports;
				Assert.AreEqual(1, exports.Test(1, 2, 3));
				Assert.AreEqual(2, exports.Test(1, 2, 0));
				Assert.AreEqual(4, exports.Test(4, 5, 1));
				Assert.AreEqual(5, exports.Test(4, 5, 0));
			}
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="ValueType.Float32"/> input.
		/// </summary>
		[TestMethod]
		public void Select_Compiled_Float32()
		{
			using (var compiled = CreateTester<float>(ValueType.Float32))
			{
				var exports = compiled.Exports;
				Assert.AreEqual(1, exports.Test(1, 2, 3));
				Assert.AreEqual(2, exports.Test(1, 2, 0));
				Assert.AreEqual(4, exports.Test(4, 5, 1));
				Assert.AreEqual(5, exports.Test(4, 5, 0));
			}
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="ValueType.Float64"/> input.
		/// </summary>
		[TestMethod]
		public void Select_Compiled_Float64()
		{
			using (var compiled = CreateTester<double>(ValueType.Float64))
			{
				var exports = compiled.Exports;
				Assert.AreEqual(1, exports.Test(1, 2, 3));
				Assert.AreEqual(2, exports.Test(1, 2, 0));
				Assert.AreEqual(4, exports.Test(4, 5, 1));
				Assert.AreEqual(5, exports.Test(4, 5, 0));
			}
		}
	}
}